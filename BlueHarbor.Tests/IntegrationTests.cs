using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Security;
using BlueHarbor.Application.Services;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Persistence;
using BlueHarbor.Infrastructure.Repositories;
using BlueHarbor.Security;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlueHarbor.Tests;

/// <summary>
/// Classe di test di integrazione per verificare il flusso completo di operazioni del porto.
/// Copre scenari di creazione, avanzamento temporale, assegnazione banchine, partenze e permessi.
/// </summary>
public class IntegrationTests
{
    /// <summary>
    /// Crea un'istanza in-memory del database per isolare ciascun test.
    /// </summary>
    private BlueHarborDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BlueHarborDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new BlueHarborDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Testa il flusso completo del ciclo di vita di una nave:
    /// Creazione -> Avanzamento del tempo -> Assegnazione banchina -> Partenza automatica.
    /// </summary>
    [Fact]
    public async Task CompleteFlow_ShouldWork()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        
        // Mocking Hangfire background job client
        var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        
        // Inizializzazione dei servizi con le dipendenze corrette
        var timeManagementService = new TimeManagementService(stateRepo, shipRepo, backgroundJobClientMock.Object);
        var shipService = new ShipService(shipRepo, stateRepo);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // 1. Verifica che il giorno iniziale di sistema sia impostato a 1
        var initialState = await stateRepo.GetAsync();
        var initialDay = initialState.CurrentDay;
        Assert.Equal(1, initialDay);

        // 2. Creazione della nave (genera automaticamente taglia, giorno arrivo e durata permanenza)
        var createRequest = new CreateShipRequest("Test Ship", "Some notes");
        var shipResponse = await shipService.CreateShipAsync(createRequest);
        
        Assert.NotNull(shipResponse);
        Assert.Equal("Test Ship", shipResponse.Name);
        Assert.Equal("Pending", shipResponse.Status);
        Assert.True(shipResponse.ArrivalDay > 1);

        // 3. Verifica che la nave creata compaia nella lista globale
        var allShipsBefore = await shipRepo.GetAllShipsAsync();
        Assert.NotEmpty(allShipsBefore);

        // 4. Avanzamento del tempo virtuale di sistema fino al giorno di arrivo della nave
        while ((await stateRepo.GetAsync()).CurrentDay < shipResponse.ArrivalDay)
        {
            await timeManagementService.AdvanceDayAsync();
        }
        
        var currentDay = (await stateRepo.GetAsync()).CurrentDay;
        Assert.Equal(shipResponse.ArrivalDay, currentDay);

        // 5. Assegnazione della banchina compatibile in base alla dimensione
        var berths = await context.Banchine.Include(b => b.Dimensione).ToListAsync();
        var compatibleBerth = berths.First(b => b.Dimensione.NomeDimensione == shipResponse.Size);
        
        var assignment = await schedulerService.AssignShipToBerthAsync(shipResponse.Id, compatibleBerth.IdBanchina);
        
        Assert.NotNull(assignment);
        Assert.Equal(shipResponse.Id, assignment.ShipId);
        Assert.Equal(compatibleBerth.IdBanchina, assignment.BerthId);
        Assert.True(assignment.StartDay >= shipResponse.ArrivalDay);

        // 6. Verifica che l'inizio dell'occupazione sia correttamente registrato nel DTO della nave
        var allShipsAfter = await shipRepo.GetAllShipsAsync();
        var shipDtoAfter = allShipsAfter.First(s => s.IdNave == shipResponse.Id);
        Assert.Equal(assignment.StartDay, shipDtoAfter.GiornoInizio);

        // 7. Verifica che lo stato della nave sia ora "Assigned"
        var updatedShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.NotNull(updatedShip);
        Assert.Equal("Assigned", updatedShip.Stato);

        // 8. Avanzamento temporale oltre il termine del periodo di occupazione per innescare la partenza
        int departureDay = assignment.StartDay + updatedShip.DurataOccupazione;
        
        var currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        while (currentSystemDay < departureDay)
        {
            await timeManagementService.AdvanceDayAsync();
            currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        }

        // Eseguiamo il metodo di processamento delle navi partite (normalmente invocato dal job Hangfire)
        await timeManagementService.ProcessDepartedShipsAsync(currentSystemDay);

        // 9. Verifica dello stato finale: la nave deve essere contrassegnata come "Departed"
        var finalShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.NotNull(finalShip);
        Assert.Equal("Departed", finalShip.Stato);
    }

    /// <summary>
    /// Verifica le policy di sicurezza per cui gli utenti con ruolo "Operatore"
    /// non possono accedere alle funzionalità dell'endpoint SchedulerController.
    /// </summary>
    [Fact]
    public async Task Security_Operator_CannotAssignBerth()
    {
        var controllerType = typeof(BlueHarbor.Controllers.SchedulerController);
        var authorizeAttr = (AuthorizeAttribute?)Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute));
        
        Assert.NotNull(authorizeAttr);
        Assert.Equal(Roles.Scheduler, authorizeAttr.Roles);
    }

    /// <summary>
    /// Verifica le policy di sicurezza per cui gli utenti con ruolo "Scheduler"
    /// non possono accedere alle funzionalità dell'endpoint ShipsController (creazione navi).
    /// </summary>
    [Fact]
    public async Task Security_Scheduler_CannotCreateShip()
    {
        var controllerType = typeof(BlueHarbor.Controllers.ShipsController);
        var authorizeAttr = (AuthorizeAttribute?)Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute));
        
        Assert.NotNull(authorizeAttr);
        Assert.Equal(Roles.Operatore, authorizeAttr.Roles);
    }

    /// <summary>
    /// Verifica che due navi assegnate alla stessa banchina non abbiano periodi di occupazione sovrapposti.
    /// L'algoritmo di pianificazione deve allocare la seconda nave al primo giorno utile dopo la partenza della prima.
    /// </summary>
    [Fact]
    public async Task OverlapPrevention_ShouldWork()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // Creiamo due navi XL (IdDimensione = 1) che arrivano lo stesso giorno
        var ship1 = new Nave { NomeNave = "XL 1", IdDimensione = 1, GiornoArrivo = 5, DurataOccupazione = 10, Stato = "Pending", IdUtente = 1 };
        var ship2 = new Nave { NomeNave = "XL 2", IdDimensione = 1, GiornoArrivo = 5, DurataOccupazione = 5, Stato = "Pending", IdUtente = 1 };
        await shipRepo.AddAsync(ship1);
        await shipRepo.AddAsync(ship2);

        var xlBerth = (await context.Banchine.ToListAsync()).First(b => b.IdDimensione == 1);

        // Act
        var assign1 = await schedulerService.AssignShipToBerthAsync(ship1.IdNave, xlBerth.IdBanchina);
        var assign2 = await schedulerService.AssignShipToBerthAsync(ship2.IdNave, xlBerth.IdBanchina);

        // Assert - La seconda nave deve partire ad occupare la banchina al termine dell'occupazione della prima
        Assert.True(assign2.StartDay >= assign1.StartDay + ship1.DurataOccupazione);
    }

    /// <summary>
    /// Verifica che il sistema prevenga l'assegnazione di una nave a una banchina non compatibile per dimensione.
    /// </summary>
    [Fact]
    public async Task SizeMismatch_ShouldThrowException()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        var shipS = new Nave { NomeNave = "Small", IdDimensione = 4, GiornoArrivo = 2, DurataOccupazione = 3, Stato = "Pending", IdUtente = 1 };
        await shipRepo.AddAsync(shipS);

        var xlBerth = (await context.Banchine.ToListAsync()).First(b => b.IdDimensione == 1);

        // Act & Assert - Atteso errore di operazione non valida a causa del mismatch di taglia
        await Assert.ThrowsAsync<InvalidOperationException>(() => schedulerService.AssignShipToBerthAsync(shipS.IdNave, xlBerth.IdBanchina));
    }

    /// <summary>
    /// Verifica che la creazione di una nave valorizzi automaticamente e in maniera casuale
    /// dimensione (XL/L/M/S), giorno di arrivo e durata entro i limiti prefissati.
    /// </summary>
    [Fact]
    public async Task CreateShip_ShouldGenerateRandomData()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, stateRepo);

        // Act
        var response = await shipService.CreateShipAsync(new CreateShipRequest("Automatic Ship", "Generated notes"));

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Automatic Ship", response.Name);
        Assert.Equal("Generated notes", response.Notes);
        Assert.Equal("Pending", response.Status);
        Assert.InRange(response.ArrivalDay, 2, 31);
        Assert.InRange(response.DurationDays, 3, 15);
    }
}
