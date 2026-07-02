using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueHarbor.Infrastructure.Repositories;

/// <summary>
/// Implementazione concreta del repository per la gestione delle Navi ed Occupazioni.
/// </summary>
public class ShipRepository(BlueHarborDbContext context) : IShipRepository
{
    public async Task<Nave?> GetByIdAsync(int id) =>
        await context.Navi
            .Include(n => n.Dimensione)
            .FirstOrDefaultAsync(n => n.IdNave == id);

    public async Task<IEnumerable<Nave>> GetByStatusAsync(string status) => 
        await context.Navi.Where(s => s.Stato == status).ToListAsync();

    /// <summary>
    /// Recupera tutte le navi che hanno lo stato impostato a "Pending".
    /// </summary>
    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await context.Navi
            .Include(n => n.Dimensione)
            .Where(s => s.Stato == "Pending")
            .Select(s => new PendingShipDto(s.IdNave, s.NomeNave, s.Dimensione.NomeDimensione, s.GiornoArrivo, s.DurataOccupazione))
            .ToListAsync();
    }

    /// <summary>
    /// Recupera tutte le navi registrate nel sistema, unendo le informazioni sulle banchine
    /// associate ad esse tramite la tabella delle occupazioni per popolare il DTO finale.
    /// </summary>
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
                // Calcola il GiornoInizio dell'occupazione cercandolo nella tabella Occupazioni
                context.Occupazioni
                    .Where(o => o.IdNave == s.IdNave)
                    .Select(o => (int?)o.GiornoInizio)
                    .FirstOrDefault(),
                // Trova l'ID della banchina assegnata
                context.Occupazioni
                    .Where(o => o.IdNave == s.IdNave)
                    .Select(o => (int?)o.IdBanchina)
                    .FirstOrDefault(),
                // Trova il nome leggibile della banchina assegnata
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

    public async Task AddAssignmentAndUpdateShipAsync(Occupazione assignment, Nave ship)
    {
        await context.Occupazioni.AddAsync(assignment);
        context.Navi.Update(ship);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Trova tutte le navi correntemente in stato "Assigned" che hanno esaurito la loro sosta in base
    /// al giorno corrente virtuale e ne aggiorna lo stato in "Departed".
    /// </summary>
    /// <param name="currentDay">Giorno virtuale corrente di sistema.</param>
    /// <returns>Il numero di righe modificate nel database.</returns>
    public async Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay)
    {
        // Seleziona le navi in stato Assigned la cui data di inizio occupazione + durata è minore o uguale al giorno corrente
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

/// <summary>
/// Implementazione concreta del repository per la gestione delle Banchine fisiche.
/// </summary>
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

    /// <summary>
    /// Recupera tutte le banchine incluse le occupazioni programmate e i dettagli delle navi assegnate,
    /// proiettando il tutto in un DTO ottimizzato per la visualizzazione della timeline.
    /// </summary>
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
                    // Fine occupazione calcolata come: inizio + durata - 1
                    a.GiornoInizio + a.Nave.DurataOccupazione - 1,
                    a.Nave.Stato
                ))
            ))
            .ToListAsync();
    }
}

/// <summary>
/// Implementazione concreta del repository per la gestione dello stato e del giorno virtuale di sistema.
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
    /// Incrementa il contatore del giorno virtuale di sistema.
    /// </summary>
    public async Task<int> AdvanceDayAsync()
    {
        var state = await context.SystemStates.FirstAsync();
        state.CurrentDay++;
        await context.SaveChangesAsync();
        return state.CurrentDay;
    }
}