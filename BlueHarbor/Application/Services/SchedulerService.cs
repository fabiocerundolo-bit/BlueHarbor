using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

public class SchedulerService(IShipRepository shipRepository, IBerthRepository berthRepository) : ISchedulerService
{
    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await shipRepository.GetPendingShipsAsync();
    }

    public async Task<IEnumerable<BerthDto>> GetBerthsAsync()
    {
        return await berthRepository.GetBerthsWithAssignmentsAsync();
    }

    public async Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId)
    {
        // 1. Recupera la nave e verifica che sia in stato Pending
        var ship = await shipRepository.GetByIdAsync(shipId);
        if (ship == null) throw new KeyNotFoundException("Nave non trovata.");
        if (ship.Stato != "Pending") 
            throw new InvalidOperationException("La nave non è in stato Pending.");

        // 2. Recupera la banchina con le sue assegnazioni esistenti
        var berth = await berthRepository.GetByIdAsync(berthId);
        if (berth == null) throw new KeyNotFoundException("Banchina non trovata.");

        // 3. Regola di dominio: la banchina deve essere compatibile per dimensione
        if (berth.IdDimensione != ship.IdDimensione)
        {
            throw new InvalidOperationException($"Dimensione non compatibile. Nave: {ship.IdDimensione}, Banchina: {berth.IdDimensione}");
        }

        // 4. Algoritmo: Trova il primo slot temporale disponibile
        int startDay = FindFirstAvailableSlot(berth, ship.GiornoArrivo, ship.DurataOccupazione);
        int endDay = startDay + ship.DurataOccupazione - 1;

        // 5. Aggiorna lo stato della nave
        ship.Stato = "Assigned";

        // 6. Crea il record di occupazione
        var occupazione = new Occupazione
        {
            IdNave = ship.IdNave,
            IdBanchina = berth.IdBanchina,
            GiornoInizio = startDay,
            IdUtente = 1 // Default Admin
        };
        
        await shipRepository.AddAssignmentAsync(occupazione);
        await shipRepository.UpdateAsync(ship);

        return new AssignmentResponseDto(ship.IdNave, berth.IdBanchina, startDay, endDay, ship.Stato);
    }

    private int FindFirstAvailableSlot(Banchina berth, int earliestStart, int duration)
    {
        int candidateStart = earliestStart;
        bool hasConflict;

        do
        {
            hasConflict = false;
            int candidateEnd = candidateStart + duration - 1;

            foreach (var existing in berth.Occupazioni)
            {
                // Calcoliamo la fine dell'occupazione esistente
                int existingEnd = existing.GiornoInizio + (existing.Nave?.DurataOccupazione ?? 0) - 1;

                if (candidateStart <= existingEnd && candidateEnd >= existing.GiornoInizio)
                {
                    hasConflict = true;
                    candidateStart = existingEnd + 1;
                    break;
                }
            }
        } while (hasConflict);

        return candidateStart;
    }
}
