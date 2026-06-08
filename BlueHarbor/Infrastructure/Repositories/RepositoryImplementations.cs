using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueHarbor.Infrastructure.Repositories;

public class ShipRepository(BlueHarborDbContext context) : IShipRepository
{
    public async Task<Nave?> GetByIdAsync(int id) => await context.Navi.FindAsync(id);

    public async Task<IEnumerable<Nave>> GetByStatusAsync(string status) => 
        await context.Navi.Where(s => s.Stato == status).ToListAsync();

    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await context.Navi
            .Include(n => n.Dimensione)
            .Where(s => s.Stato == "Pending")
            .Select(s => new PendingShipDto(s.IdNave, s.NomeNave, s.Dimensione.NomeDimensione, s.GiornoArrivo, s.DurataOccupazione))
            .ToListAsync();
    }

    public async Task<IEnumerable<ShipDto>> GetAllShipsAsync()
    {
        return await context.Navi
            .Include(n => n.Dimensione)
            .Include(n => n.Utente)
            .OrderByDescending(s => s.IdNave)
            .Select(s => new ShipDto(
                s.IdNave, 
                s.NomeNave, 
                s.Note, 
                s.Dimensione.NomeDimensione, 
                s.GiornoArrivo, 
                s.DurataOccupazione, 
                s.Stato,
                context.Occupazioni
                    .Where(o => o.IdNave == s.IdNave)
                    .Select(o => (int?)o.GiornoInizio)
                    .FirstOrDefault(),
                context.Occupazioni
                    .Where(o => o.IdNave == s.IdNave)
                    .Select(o => (int?)o.IdBanchina)
                    .FirstOrDefault(),
                context.Occupazioni
                    .Where(o => o.IdNave == s.IdNave)
                    .Select(o => o.Banchina.NomeBanchina)
                    .FirstOrDefault()
            ))
            .ToListAsync();
    }

    public async Task AddAsync(Nave ship)
    {
        await context.Navi.AddAsync(ship);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Nave ship)
    {
        context.Navi.Update(ship);
        await context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<Nave> ships)
    {
        context.Navi.UpdateRange(ships);
        await context.SaveChangesAsync();
    }

    public async Task AddAssignmentAsync(Occupazione assignment)
    {
        await context.Occupazioni.AddAsync(assignment);
        await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay)
    {
        var shipsToDepart = await context.Navi
            .Where(s => s.Stato == "Assigned")
            .Join(context.Occupazioni, n => n.IdNave, o => o.IdNave, (n, o) => new { n, o })
            .Where(x => (x.o.GiornoInizio + x.n.DurataOccupazione) <= currentDay)
            .Select(x => x.n)
            .ToListAsync();

        foreach (var ship in shipsToDepart)
        {
            ship.Stato = "Departed";
        }

        return await context.SaveChangesAsync();
    }
}

public class BerthRepository(BlueHarborDbContext context) : IBerthRepository
{
    public async Task<Banchina?> GetByIdAsync(int id) => 
        await context.Banchine
            .Include(b => b.Dimensione)
            .Include(b => b.Occupazioni)
            .ThenInclude(o => o.Nave)
            .FirstOrDefaultAsync(b => b.IdBanchina == id);

    public async Task<IEnumerable<Banchina>> GetAllWithAssignmentsAsync() => 
        await context.Banchine
            .Include(b => b.Dimensione)
            .Include(b => b.Occupazioni)
            .ToListAsync();

    public async Task<IEnumerable<BerthDto>> GetBerthsWithAssignmentsAsync()
    {
        return await context.Banchine
            .Include(b => b.Dimensione)
            .Include(b => b.Occupazioni)
            .ThenInclude(a => a.Nave)
            .Select(b => new BerthDto(
                b.IdBanchina,
                b.NomeBanchina,
                b.Dimensione.NomeDimensione,
                b.Occupazioni.Select(a => new BerthAssignmentDto(
                    a.IdOccupazione,
                    a.IdNave,
                    a.Nave.NomeNave,
                    a.GiornoInizio,
                    a.GiornoInizio + a.Nave.DurataOccupazione - 1,
                    a.Nave.Stato
                ))
            ))
            .ToListAsync();
    }
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
