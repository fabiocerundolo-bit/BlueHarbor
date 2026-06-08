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

    public async Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId)
    {
        // 1. Recupera la nave e verifica che sia in stato Pending
        var ship = await shipRepository.GetByIdAsync(shipId);
        if (ship == null) throw new KeyNotFoundException("Nave non trovata.");
        if (ship.Status != ShipStatus.Pending) 
            throw new InvalidOperationException("La nave non è in stato Pending.");

        // 2. Recupera la banchina con le sue assegnazioni esistenti
        var berth = await berthRepository.GetByIdAsync(berthId);
        if (berth == null) throw new KeyNotFoundException("Banchina non trovata.");

        // 3. Regola di dominio: la banchina deve essere compatibile per dimensione
        if (berth.Size != ship.Size)
        {
            throw new InvalidOperationException($"Dimensione non compatibile. Nave: {ship.Size}, Banchina: {berth.Size}");
        }

        // 4. Algoritmo: Trova il primo slot temporale disponibile
        int startDay = FindFirstAvailableSlot(berth, ship.ArrivalDay, ship.DurationDays);
        int endDay = startDay + ship.DurationDays - 1;

        // 5. Aggiorna lo stato della nave e le proprietà di assegnazione
        ship.Status = ShipStatus.Assigned;
        ship.AssignedBerthId = berth.Id;
        ship.StartDay = startDay;

        // 6. Crea il record di assegnazione
        var assignment = new Assignment
        {
            ShipId = ship.Id,
            BerthId = berth.Id,
            StartDay = startDay,
            EndDay = endDay
        };
        
        await shipRepository.AddAssignmentAsync(assignment);
        await shipRepository.UpdateAsync(ship);

        return new AssignmentResponseDto(ship.Id, berth.Id, startDay, endDay, ship.Status);
    }

    private int FindFirstAvailableSlot(Berth berth, int earliestStart, int duration)
    {
        int candidateStart = earliestStart;
        bool hasConflict;

        do
        {
            hasConflict = false;
            int candidateEnd = candidateStart + duration - 1;

            foreach (var existing in berth.Assignments)
            {
                // Condizione di sovrapposizione: 
                // Il nuovo intervallo [candidateStart, candidateEnd] si sovrappone a [existing.StartDay, existing.EndDay]
                // se candidateStart <= existing.EndDay E candidateEnd >= existing.StartDay
                if (candidateStart <= existing.EndDay && candidateEnd >= existing.StartDay)
                {
                    hasConflict = true;
                    // Sposta il candidato al giorno successivo alla fine dell'assegnazione conflittuale
                    candidateStart = existing.EndDay + 1;
                    break; // Ricomincia il controllo con il nuovo candidateStart
                }
            }
        } while (hasConflict);

        return candidateStart;
    }
}
