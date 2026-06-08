using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueHarbor.Infrastructure.Repositories;

public class ShipRepository(BlueHarborDbContext context) : IShipRepository
{
    public async Task<Ship?> GetByIdAsync(int id) => await context.Ships.FindAsync(id);

    public async Task<IEnumerable<Ship>> GetByStatusAsync(ShipStatus status) => 
        await context.Ships.Where(s => s.Status == status).ToListAsync();

    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await context.Ships
            .Where(s => s.Status == ShipStatus.Pending)
            .Select(s => new PendingShipDto(s.Id, s.Name, s.Size, s.ArrivalDay, s.DurationDays))
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

    public async Task AddAssignmentAsync(Assignment assignment)
    {
        await context.Assignments.AddAsync(assignment);
        await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay)
    {
        var shipsToDepart = await context.Ships
            .Where(s => s.Status == ShipStatus.Assigned && 
                        s.StartDay.HasValue && 
                        (s.StartDay.Value + s.DurationDays) <= currentDay)
            .ToListAsync();

        foreach (var ship in shipsToDepart)
        {
            ship.Status = ShipStatus.Departed;
        }

        return await context.SaveChangesAsync();
    }
}

public class BerthRepository(BlueHarborDbContext context) : IBerthRepository
{
    public async Task<Berth?> GetByIdAsync(int id) => 
        await context.Berths.Include(b => b.Assignments).FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Berth>> GetAllWithAssignmentsAsync() => 
        await context.Berths.Include(b => b.Assignments).ToListAsync();
}

public class SystemStateRepository(BlueHarborDbContext context) : ISystemStateRepository
{
    public async Task<SystemState> GetAsync() => 
        await context.SystemStates.FirstAsync();

    public async Task UpdateAsync(SystemState state)
    {
        context.SystemStates.Update(state);
        await context.SaveChangesAsync();
    }

    public async Task<int> AdvanceDayAsync()
    {
        var state = await context.SystemStates.FirstAsync();
        state.CurrentDay++;
        await context.SaveChangesAsync();
        return state.CurrentDay;
    }
}
