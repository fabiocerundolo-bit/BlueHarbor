using BlueHarbor.Application.DTOs;

namespace BlueHarbor.Application.Interfaces;

/// <summary>
/// Interface for the ship management service (registration).
/// </summary>
public interface IShipService
{
    /// <summary>
    /// Creates and registers a new ship in the system with randomly generated attributes.
    /// </summary>
    Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request);
}

/// <summary>
/// Interface for the time management service and virtual day advancement.
/// </summary>
public interface ITimeManagementService
{
    /// <summary>
    /// Advances the virtual system day by 1.
    /// </summary>
    Task<int> AdvanceDayAsync();

    /// <summary>
    /// Processes ships whose occupancy expires on the specified day and marks them as "Departed".
    /// </summary>
    Task ProcessDepartedShipsAsync(int currentDay);
}

/// <summary>
/// Interface for the berth scheduling service.
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Returns the list of ships waiting to be assigned.
    /// </summary>
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();

    /// <summary>
    /// Returns the list of berths with their respective scheduled occupancies.
    /// </summary>
    Task<IEnumerable<BerthDto>> GetBerthsAsync();

    /// <summary>
    /// Assigns a pending ship to a specific berth.
    /// </summary>
    Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId);
}