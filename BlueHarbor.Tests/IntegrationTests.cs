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
/// Integration test class to verify the complete flow of harbor operations.
/// Covers scenarios for ship creation, time advancement, berth assignment, departures, and permissions.
/// </summary>
public class IntegrationTests
{
    /// <summary>
    /// Creates an in-memory database instance to isolate each test.
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
    /// Tests the complete ship lifecycle flow:
    /// Creation -> Time Advancement -> Berth Assignment -> Automatic Departure.
    /// </summary>
    [Fact]
    public async Task CompleteFlow_ShouldWork()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var listaNaviRepo = new ListaNaviRepository(context);
        var berthRepo = new BerthRepository(context);
        var stateRepo = new SystemStateRepository(context);
        
        // Mocking Hangfire background job client
        var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        
        // Initialize services with correct dependencies
        var timeManagementService = new TimeManagementService(stateRepo, shipRepo, backgroundJobClientMock.Object);
        var shipService = new ShipService(shipRepo, listaNaviRepo, stateRepo);
        var schedulerService = new SchedulerService(shipRepo, berthRepo);

        // 1. Verify the initial system day is set to 1
        var initialState = await stateRepo.GetAsync();
        var initialDay = initialState.CurrentDay;
        Assert.Equal(1, initialDay);

        // 2. Create a ship (automatically generates size, arrival day, and stay duration)
        var createRequest = new CreateShipRequest(1, "Some notes");
        var shipResponse = await shipService.CreateShipAsync(createRequest);
        
        Assert.NotNull(shipResponse);
        Assert.Equal("MSC Splendida", shipResponse.Name);
        Assert.Equal("Pending", shipResponse.Status);
        Assert.True(shipResponse.ArrivalDay > 1);

        // 3. Verify that the created ship appears in the global list
        var allShipsBefore = await shipRepo.GetAllShipsAsync();
        Assert.NotEmpty(allShipsBefore);

        // 4. Advance the virtual system time to the ship's arrival day
        while ((await stateRepo.GetAsync()).CurrentDay < shipResponse.ArrivalDay)
        {
            await timeManagementService.AdvanceDayAsync();
        }
        
        var currentDay = (await stateRepo.GetAsync()).CurrentDay;
        Assert.Equal(shipResponse.ArrivalDay, currentDay);

        // 5. Assign a compatible berth based on size
        var berths = await context.Berths.Include(b => b.Size).ToListAsync();
        var compatibleBerth = berths.First(b => b.Size.SizeName == shipResponse.Size);
        
        var assignment = await schedulerService.AssignShipToBerthAsync(shipResponse.Id, compatibleBerth.BerthId);
        
        Assert.NotNull(assignment);
        Assert.Equal(shipResponse.Id, assignment.ShipId);
        Assert.Equal(compatibleBerth.BerthId, assignment.BerthId);
        Assert.True(assignment.StartDay >= shipResponse.ArrivalDay);

        // 6. Verify that the occupancy start day is correctly recorded in the ship DTO
        var allShipsAfter = await shipRepo.GetAllShipsAsync();
        var shipDtoAfter = allShipsAfter.First(s => s.ShipId == shipResponse.Id);
        Assert.Equal(assignment.StartDay, shipDtoAfter.StartDay);

        // 7. Verify that the ship status is now "Assigned"
        var updatedShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.NotNull(updatedShip);
        Assert.Equal("Assigned", updatedShip.Status);

        // 8. Advance time beyond the end of the occupancy period to trigger departure
        int departureDay = assignment.StartDay + updatedShip.DurationDays;
        
        var currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        while (currentSystemDay < departureDay)
        {
            await timeManagementService.AdvanceDayAsync();
            currentSystemDay = (await stateRepo.GetAsync()).CurrentDay;
        }

        // Execute the departed ships processing method (normally invoked by the Hangfire job)
        await timeManagementService.ProcessDepartedShipsAsync(currentSystemDay);

        // 9. Final status check: the ship must be marked as "Departed"
        var finalShip = await shipRepo.GetByIdAsync(shipResponse.Id);
        Assert.NotNull(finalShip);
        Assert.Equal("Departed", finalShip.Status);
    }

    /// <summary>
    /// Verifies the security policy that users with the "Operator" role
    /// cannot access the SchedulerController endpoint functionality.
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
    /// Verifies the security policy that users with the "Scheduler" role
    /// cannot access the ShipsController endpoint functionality (ship creation).
    /// The class-level attribute allows both Operator and Scheduler to call GET /ships,
    /// while POST /ships is restricted to Operator only at the method level.
    /// </summary>
    [Fact]
    public async Task Security_Scheduler_CannotCreateShip()
    {
        var methodInfo = typeof(BlueHarbor.Controllers.ShipsController)
            .GetMethod(nameof(BlueHarbor.Controllers.ShipsController.CreateShip));
        var authorizeAttr = (AuthorizeAttribute?)Attribute.GetCustomAttribute(methodInfo!, typeof(AuthorizeAttribute));
        
        Assert.NotNull(authorizeAttr);
        Assert.Equal(Roles.Operator, authorizeAttr.Roles);
    }

    /// <summary>
    /// Verifies that two ships assigned to the same berth do not have overlapping occupancy periods.
    /// The scheduling algorithm must allocate the second ship to the first available day after the first ship departs.
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

        // Create two XL ships (SizeId = 1) that arrive on the same day
        var ship1 = new Ship { IdListaNavi = 1, ArrivalDay = 5, DurationDays = 10, Status = "Pending", UserId = 1 };
        var ship2 = new Ship { IdListaNavi = 2, ArrivalDay = 5, DurationDays = 5, Status = "Pending", UserId = 1 };
        await shipRepo.AddAsync(ship1);
        await shipRepo.AddAsync(ship2);

        var xlBerth = (await context.Berths.ToListAsync()).First(b => b.SizeId == 1);

        // Act
        var assign1 = await schedulerService.AssignShipToBerthAsync(ship1.ShipId, xlBerth.BerthId);
        var assign2 = await schedulerService.AssignShipToBerthAsync(ship2.ShipId, xlBerth.BerthId);

        // Assert - The second ship must start occupying the berth after the first ship's occupancy ends
        Assert.True(assign2.StartDay >= assign1.StartDay + ship1.DurationDays);
    }

    /// <summary>
    /// Verifies that the system prevents assigning a ship to a size-incompatible berth.
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

        var shipS = new Ship { IdListaNavi = 7, ArrivalDay = 2, DurationDays = 3, Status = "Pending", UserId = 1 };
        await shipRepo.AddAsync(shipS);

        var xlBerth = (await context.Berths.ToListAsync()).First(b => b.SizeId == 1);

        // Act & Assert - Expected invalid operation error due to size mismatch
        await Assert.ThrowsAsync<InvalidOperationException>(() => schedulerService.AssignShipToBerthAsync(shipS.ShipId, xlBerth.BerthId));
    }

    /// <summary>
    /// Verifies that creating a ship automatically and randomly populates
    /// size (XL/L/M/S), arrival day, and duration within the preset limits.
    /// </summary>
    [Fact]
    public async Task CreateShip_ShouldGenerateRandomData()
    {
        // Arrange
        var context = GetDbContext();
        var shipRepo = new ShipRepository(context);
        var listaNaviRepo = new ListaNaviRepository(context);
        var stateRepo = new SystemStateRepository(context);
        var shipService = new ShipService(shipRepo, listaNaviRepo, stateRepo);

        // Act
        var response = await shipService.CreateShipAsync(new CreateShipRequest(1, "Generated notes"));

        // Assert
        Assert.NotNull(response);
        Assert.Equal("MSC Splendida", response.Name);
        Assert.Equal("Generated notes", response.Notes);
        Assert.Equal("Pending", response.Status);
        Assert.InRange(response.ArrivalDay, 2, 31);
        Assert.InRange(response.DurationDays, 3, 15);
    }
}
