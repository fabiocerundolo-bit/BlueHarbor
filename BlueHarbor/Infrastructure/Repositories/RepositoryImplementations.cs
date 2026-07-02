using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueHarbor.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the repository for managing Ships and Occupancies.
/// </summary>
public class ShipRepository(BlueHarborDbContext context) : IShipRepository
{
    public async Task<Ship?> GetByIdAsync(int id) =>
        await context.Ships
            .Include(n => n.Size)
            .FirstOrDefaultAsync(n => n.ShipId == id);

    public async Task<IEnumerable<Ship>> GetByStatusAsync(string status) => 
        await context.Ships.Where(s => s.Status == status).ToListAsync();

    /// <summary>
    /// Retrieves all ships with status set to "Pending".
    /// </summary>
    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await context.Ships
            .Include(n => n.Size)
            .Where(s => s.Status == "Pending")
            .Select(s => new PendingShipDto(s.ShipId, s.ShipName, s.Size.SizeName, s.ArrivalDay, s.DurationDays))
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all ships registered in the system, joining berth information
    /// via the occupancy table to populate the final DTO.
    /// </summary>
    public async Task<IEnumerable<ShipDto>> GetAllShipsAsync()
    {
        return await context.Ships
            .Include(n => n.Size)
            .Include(n => n.User)
            .OrderByDescending(s => s.ShipId)
            .Select(s => new ShipDto(
                s.ShipId, 
                s.ShipName, 
                s.Notes, 
                s.Size.SizeName, 
                s.ArrivalDay, 
                s.DurationDays, 
                s.Status,
                // Calculate the StartDay of the occupancy by looking it up in the Occupancy table
                context.Occupancies
                    .Where(o => o.ShipId == s.ShipId)
                    .Select(o => (int?)o.StartDay)
                    .FirstOrDefault(),
                // Find the ID of the assigned berth
                context.Occupancies
                    .Where(o => o.ShipId == s.ShipId)
                    .Select(o => (int?)o.BerthId)
                    .FirstOrDefault(),
                // Find the human-readable name of the assigned berth
                context.Occupancies
                    .Where(o => o.ShipId == s.ShipId)
                    .Select(o => o.Berth.BerthName)
                    .FirstOrDefault()
            ))
            .ToListAsync();
    }

    public async Task AddAsync(Ship ship)
    {
        await context.Ships.AddAsync(ship);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ship ship)
    {
        context.Ships.Update(ship);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Ship> ships)
    {
        context.Ships.UpdateRange(ships);
        await context.SaveChangesAsync();
    }

    public async Task AddAssignmentAsync(Occupancy assignment)
    {
        await context.Occupancies.AddAsync(assignment);
        await context.SaveChangesAsync();
    }

    public async Task AddAssignmentAndUpdateShipAsync(Occupancy assignment, Ship ship)
    {
        await context.Occupancies.AddAsync(assignment);
        context.Ships.Update(ship);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Finds all ships currently in "Assigned" status that have exhausted their stay based on
    /// the current virtual day, and updates their status to "Departed".
    /// </summary>
    /// <param name="currentDay">Current virtual system day.</param>
    /// <returns>The number of rows modified in the database.</returns>
    public async Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay)
    {
        // Select ships in Assigned status whose start day + duration is less than or equal to the current day
        var shipsToDepart = await context.Ships
            .Where(s => s.Status == "Assigned")
            .Join(context.Occupancies, n => n.ShipId, o => o.ShipId, (n, o) => new { n, o })
            .Where(x => (x.o.StartDay + x.n.DurationDays) <= currentDay)
            .Select(x => x.n)
            .ToListAsync();

        foreach (var ship in shipsToDepart)
        {
            ship.Status = "Departed";
        }

        return await context.SaveChangesAsync();
    }
}

/// <summary>
/// Concrete implementation of the repository for managing physical Berths.
/// </summary>
public class BerthRepository(BlueHarborDbContext context) : IBerthRepository
{
    public async Task<Berth?> GetByIdAsync(int id) => 
        await context.Berths
            .Include(b => b.Size)
            .Include(b => b.Occupancies)
            .ThenInclude(o => o.Ship)
            .FirstOrDefaultAsync(b => b.BerthId == id);

    public async Task<IEnumerable<Berth>> GetAllWithAssignmentsAsync() => 
        await context.Berths
            .Include(b => b.Size)
            .Include(b => b.Occupancies)
            .ToListAsync();

    /// <summary>
    /// Retrieves all berths including their scheduled occupancies and assigned ship details,
    /// projecting everything into a DTO optimised for timeline display.
    /// </summary>
    public async Task<IEnumerable<BerthDto>> GetBerthsWithAssignmentsAsync()
    {
        return await context.Berths
            .Include(b => b.Size)
            .Include(b => b.Occupancies)
            .ThenInclude(a => a.Ship)
            .Select(b => new BerthDto(
                b.BerthId,
                b.BerthName,
                b.Size.SizeName,
                b.Occupancies.Select(a => new BerthAssignmentDto(
                    a.OccupancyId,
                    a.ShipId,
                    a.Ship.ShipName,
                    a.StartDay,
                    // End occupancy calculated as: start + duration - 1
                    a.StartDay + a.Ship.DurationDays - 1,
                    a.Ship.Status
                ))
            ))
            .ToListAsync();
    }
}

/// <summary>
/// Concrete implementation of the repository for managing the system state and virtual day.
/// </summary>
public class SystemStateRepository(BlueHarborDbContext context) : ISystemStateRepository
{
    public async Task<SystemState> GetAsync() => 
        await context.SystemStates.FirstAsync();

    public async Task UpdateAsync(SystemState state)
    {
        context.SystemStates.Update(state);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Increments the virtual system day counter.
    /// </summary>
    public async Task<int> AdvanceDayAsync()
    {
        var state = await context.SystemStates.FirstAsync();
        state.CurrentDay++;
        await context.SaveChangesAsync();
        return state.CurrentDay;
    }
}