using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Infrastructure.Repositories;

/// <summary>
/// Repository interface for the Ship entity.
/// Manages persistence and queries related to ships and their berth assignments.
/// </summary>
public interface IShipRepository
{
    /// <summary>
    /// Retrieves a ship by its unique ID.
    /// </summary>
    Task<Ship?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all ships that are in a specific status.
    /// </summary>
    Task<IEnumerable<Ship>> GetByStatusAsync(string status);

    /// <summary>
    /// Retrieves pending ships projected into a lightweight DTO for the scheduler.
    /// </summary>
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();

    /// <summary>
    /// Retrieves the list of all registered ships projected into ShipDto with occupancy details.
    /// </summary>
    Task<IEnumerable<ShipDto>> GetAllShipsAsync();

    /// <summary>
    /// Adds a new ship to the database.
    /// </summary>
    Task AddAsync(Ship ship);

    /// <summary>
    /// Updates an existing ship's data.
    /// </summary>
    Task UpdateAsync(Ship ship);

    /// <summary>
    /// Updates a range of ships.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<Ship> ships);

    /// <summary>
    /// Registers the temporal assignment (occupancy) of a ship to a berth.
    /// </summary>
    Task AddAssignmentAsync(Occupancy assignment);

    /// <summary>
    /// Saves occupancy and ship status update in a single atomic transaction.
    /// </summary>
    Task AddAssignmentAndUpdateShipAsync(Occupancy assignment, Ship ship);

    /// <summary>
    /// Finds assigned ships whose stay has ended on the specified day and updates their status to "Departed".
    /// </summary>
    Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay);
}

/// <summary>
/// Repository interface for the Berth entity.
/// </summary>
public interface IBerthRepository
{
    /// <summary>
    /// Retrieves a berth by ID including its occupancies and ship details.
    /// </summary>
    Task<Berth?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all berths including size and occupancy relationships.
    /// </summary>
    Task<IEnumerable<Berth>> GetAllWithAssignmentsAsync();

    /// <summary>
    /// Retrieves all berths formatted as BerthDto for the user interface.
    /// </summary>
    Task<IEnumerable<BerthDto>> GetBerthsWithAssignmentsAsync();
}

/// <summary>
/// Repository interface for managing the global system state and virtual harbor time.
/// </summary>
public interface ISystemStateRepository
{
    /// <summary>
    /// Retrieves the single system state record.
    /// </summary>
    Task<SystemState> GetAsync();

    /// <summary>
    /// Updates the system state record.
    /// </summary>
    Task UpdateAsync(SystemState state);

    /// <summary>
    /// Increments the virtual system day counter by 1.
    /// </summary>
    Task<int> AdvanceDayAsync();
}
