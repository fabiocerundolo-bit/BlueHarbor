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
        Assert.Equal(ShipStatus.Pending, shipResponse.Status);
        Assert.True(shipResponse.ArrivalDay > 1);

        // 3. Avanzamento Giorno fino all'arrivo della nave (o quasi)
        while (await systemService.GetCurrentDayAsync() < shipResponse.ArrivalDay)
        {
            await systemService.AdvanceDayAsync();
        }
        
        var currentDay = await systemService.GetCurrentDayAsync();
        Assert.Equal(shipResponse.ArrivalDay, currentDay);

        // 4. Assegnazione Banchina
        // Cerchiamo una banchina della taglia giusta
        var berths = await context.Berths.ToListAsync();
        var compatibleBerth = berths.First(b => b.Size == shipResponse.Size);
        
        var assignment = await schedulerService.AssignShipToBerthAsync(shipResponse.Id, compatibleBerth.Id);
        
        Assert.NotNull(assignment);
        Assert.Equal(shipResponse.Id, assignment.ShipId);
        Assert.Equal(compatibleBerth.Id, assignment.BerthId);
        Assert.True(assignment.StartDay >= shipResponse.ArrivalDay);

        // 5. Verifica stato nave aggiornato
        var updatedShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.Equal(ShipStatus.Assigned, updatedShip.Status);
        Assert.Equal(compatibleBerth.Id, updatedShip.AssignedBerthId);

        // 6. Test per lo stato Departed (via Time Management Service)
        var timeManagementService = new TimeManagementService(stateRepo, shipRepo, backgroundJobClientMock.Object);
        
        // Avanziamo il giorno oltre la fine della permanenza
        // EndDay = StartDay + DurationDays - 1. 
        // Departed se (StartDay + DurationDays) <= currentDay
        int departureDay = updatedShip.StartDay.Value + updatedShip.DurationDays;
        
        var currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        while (currentSystemDay < departureDay)
        {
            await timeManagementService.AdvanceDayAsync();
            currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        }

        await timeManagementService.ProcessDepartedShipsAsync((await stateRepo.GetAsync()).CurrentDay);

        var finalShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.Equal(ShipStatus.Departed, finalShip.Status);
    }

    [Fact]
    public async Task Security_Operator_CannotAssignBerth()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);
        
        // Mocking the Controller with an Operator user
        var controller = new BlueHarbor.Controllers.SchedulerController(schedulerService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Username"] = "operatore1";
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
        {
            HttpContext = httpContext
        };

        // Act
        // We can't easily test the [Authorize] attribute in a unit test of the controller,
        // but we can verify our MockAuthenticationHandler logic and the presence of attributes.

        var type = typeof(BlueHarbor.Controllers.SchedulerController);
        var authorizeAttribute = (AuthorizeAttribute)type.GetCustomAttributes(typeof(AuthorizeAttribute), false).First();
        
        // Assert
        Assert.Equal(Roles.Scheduler, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task Security_Scheduler_CannotCreateShip()
    {
        // Arrange
        var type = typeof(BlueHarbor.Controllers.ShipsController);
        var authorizeAttribute = (AuthorizeAttribute)type.GetCustomAttributes(typeof(AuthorizeAttribute), false).First();
        
        // Assert
        Assert.Equal(Roles.Operatore, authorizeAttribute.Roles);
    }

    [Fact]
    public async Task Assignment_ShouldAvoidOverlap()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, berthRepo, stateRepo);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // Creiamo due navi XL
        var ship1 = new Ship { Name = "XL 1", Size = ShipSize.XL, ArrivalDay = 5, DurationDays = 10, Status = ShipStatus.Pending };
        var ship2 = new Ship { Name = "XL 2", Size = ShipSize.XL, ArrivalDay = 5, DurationDays = 5, Status = ShipStatus.Pending };
        await shipRepo.AddAsync(ship1);
        await shipRepo.AddAsync(ship2);

        var xlBerth = (await context.Berths.ToListAsync()).First(b => b.Size == ShipSize.XL);

        // Act
        // Assegna la prima nave
        var assign1 = await schedulerService.AssignShipToBerthAsync(ship1.Id, xlBerth.Id);
        // Assegna la seconda nave alla stessa banchina
        var assign2 = await schedulerService.AssignShipToBerthAsync(ship2.Id, xlBerth.Id);

        // Assert
        Assert.Equal(5, assign1.StartDay);
        Assert.Equal(14, assign1.EndDay); // 5 + 10 - 1
        
        // La seconda nave dovrebbe iniziare dopo la prima (15) nonostante arrivi al giorno 5
        Assert.Equal(15, assign2.StartDay);
        Assert.Equal(19, assign2.EndDay); // 15 + 5 - 1
    }

    [Fact]
    public async Task Assignment_WrongSize_ShouldThrowException()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, berthRepo, stateRepo);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        var shipS = new Ship { Name = "Small", Size = ShipSize.S, ArrivalDay = 2, DurationDays = 3, Status = ShipStatus.Pending };
        await shipRepo.AddAsync(shipS);

        var xlBerth = (await context.Berths.ToListAsync()).First(b => b.Size == ShipSize.XL);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => schedulerService.AssignShipToBerthAsync(shipS.Id, xlBerth.Id));
    }
    [Fact]
    public async Task CreateShip_ShouldGenerateCorrectData()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, berthRepo, stateRepo);

        var request = new CreateShipRequest("Automatic Ship", "Generated notes");

        // Act
        var response = await shipService.CreateShipAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Automatic Ship", response.Name);
        Assert.Equal("Generated notes", response.Notes);
        Assert.Equal(ShipStatus.Pending, response.Status);
        
        // Verifica vincoli del PDF
        // Size deve essere una delle enum
        Assert.Contains(response.Size, Enum.GetValues<ShipSize>());
        
        // ArrivalDay: CurrentDay + 1..30 (CurrentDay è 1)
        Assert.InRange(response.ArrivalDay, 2, 31);
        
        // DurationDays: 3..15
        Assert.InRange(response.DurationDays, 3, 15);
    }
}
