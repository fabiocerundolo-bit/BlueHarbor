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

public class IntegrationTests
{
    private BlueHarborDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<BlueHarborDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new BlueHarborDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CompleteFlow_ShouldWork()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        
        var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        var systemService = new SystemService(stateRepo, backgroundJobClientMock.Object);
        var shipService = new ShipService(shipRepo, berthRepo, stateRepo);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // 1. Verifica giorno iniziale
        var initialDay = await systemService.GetCurrentDayAsync();
        Assert.Equal(1, initialDay);

        // 2. Creazione Nave
        var createRequest = new CreateShipRequest("Test Ship", "Some notes");
        var shipResponse = await shipService.CreateShipAsync(createRequest);
        
        Assert.NotNull(shipResponse);
        Assert.Equal("Test Ship", shipResponse.Name);
        Assert.Equal("Pending", shipResponse.Status);
        Assert.True(shipResponse.ArrivalDay > 1);

        // 3. Verifica GetAllShipsAsync prima dell'assegnazione
        var allShipsBefore = await shipRepo.GetAllShipsAsync();
        Assert.NotEmpty(allShipsBefore);

        // 4. Avanzamento Giorno fino all'arrivo della nave (o quasi)
        while (await systemService.GetCurrentDayAsync() < shipResponse.ArrivalDay)
        {
            await systemService.AdvanceDayAsync();
        }
        
        var currentDay = await systemService.GetCurrentDayAsync();
        Assert.Equal(shipResponse.ArrivalDay, currentDay);

        // 5. Assegnazione Banchina
        // Cerchiamo una banchina della taglia giusta
        var berths = await context.Banchine.Include(b => b.Dimensione).ToListAsync();
        var compatibleBerth = berths.First(b => b.Dimensione.NomeDimensione == shipResponse.Size);
        
        var assignment = await schedulerService.AssignShipToBerthAsync(shipResponse.Id, compatibleBerth.IdBanchina);
        
        Assert.NotNull(assignment);
        Assert.Equal(shipResponse.Id, assignment.ShipId);
        Assert.Equal(compatibleBerth.IdBanchina, assignment.BerthId);
        Assert.True(assignment.StartDay >= shipResponse.ArrivalDay);

        // 6. Verifica GetAllShipsAsync dopo l'assegnazione
        var allShipsAfter = await shipRepo.GetAllShipsAsync();
        var shipDtoAfter = allShipsAfter.First(s => s.IdNave == shipResponse.Id);
        Assert.Equal(assignment.StartDay, shipDtoAfter.GiornoInizio);

        // 7. Verifica stato nave aggiornato
        var updatedShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.Equal("Assigned", updatedShip.Stato);

        // 8. Test per lo stato Departed (via BackgroundJobService)
        var backgroundJobService = new BackgroundJobService(shipRepo);
        
        // Avanziamo il giorno oltre la fine della permanenza
        int departureDay = assignment.StartDay + updatedShip.DurataOccupazione;
        
        var currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        while (currentSystemDay < departureDay)
        {
            await systemService.AdvanceDayAsync();
            currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        }

        // Eseguiamo manualmente il job di background
        await backgroundJobService.ProcessDepartedShipsAsync(currentSystemDay);

        // 9. Verifica stato finale
        var finalShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.Equal("Departed", finalShip.Stato);
    }

    [Fact]
    public async Task Security_Operator_CannotAssignBerth()
    {
        var controllerType = typeof(BlueHarbor.Controllers.SchedulerController);
        var authorizeAttr = (AuthorizeAttribute)Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute));
        
        Assert.NotNull(authorizeAttr);
        Assert.Equal(Roles.Scheduler, authorizeAttr.Roles);
    }

    [Fact]
    public async Task Security_Scheduler_CannotCreateShip()
    {
        var controllerType = typeof(BlueHarbor.Controllers.ShipsController);
        var authorizeAttr = (AuthorizeAttribute)Attribute.GetCustomAttribute(controllerType, typeof(AuthorizeAttribute));
        
        Assert.NotNull(authorizeAttr);
        Assert.Equal(Roles.Operatore, authorizeAttr.Roles);
    }

    [Fact]
    public async Task OverlapPrevention_ShouldWork()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // Creiamo due navi XL (IdDimensione = 1)
        var ship1 = new Nave { NomeNave = "XL 1", IdDimensione = 1, GiornoArrivo = 5, DurataOccupazione = 10, Stato = "Pending", IdUtente = 1 };
        var ship2 = new Nave { NomeNave = "XL 2", IdDimensione = 1, GiornoArrivo = 5, DurataOccupazione = 5, Stato = "Pending", IdUtente = 1 };
        await shipRepo.AddAsync(ship1);
        await shipRepo.AddAsync(ship2);

        var xlBerth = (await context.Banchine.ToListAsync()).First(b => b.IdDimensione == 1);

        // Act
        var assign1 = await schedulerService.AssignShipToBerthAsync(ship1.IdNave, xlBerth.IdBanchina);
        var assign2 = await schedulerService.AssignShipToBerthAsync(ship2.IdNave, xlBerth.IdBanchina);

        // Assert
        Assert.True(assign2.StartDay >= assign1.StartDay + ship1.DurataOccupazione);
    }

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

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => schedulerService.AssignShipToBerthAsync(shipS.IdNave, xlBerth.IdBanchina));
    }

    [Fact]
    public async Task CreateShip_ShouldGenerateRandomData()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, berthRepo, stateRepo);

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
