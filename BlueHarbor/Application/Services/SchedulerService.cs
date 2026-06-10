using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Servizio responsabile della gestione e pianificazione delle navi sulle banchine del porto.
/// Contiene la logica di business per verificare la compatibilità delle dimensioni e allocare slot temporali non sovrapposti.
/// </summary>
public class SchedulerService(IShipRepository shipRepository, IBerthRepository berthRepository) : ISchedulerService
{
    /// <summary>
    /// Recupera tutte le navi in attesa di essere pianificate (in stato "Pending").
    /// </summary>
    /// <returns>Una lista di DTO contenente le informazioni delle navi in attesa.</returns>
    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await shipRepository.GetPendingShipsAsync();
    }

    /// <summary>
    /// Recupera tutte le banchine disponibili assieme ai loro dettagli di occupazione programmati.
    /// </summary>
    /// <returns>Una lista di DTO rappresentanti le banchine e le rispettive assegnazioni.</returns>
    public async Task<IEnumerable<BerthDto>> GetBerthsAsync()
    {
        return await berthRepository.GetBerthsWithAssignmentsAsync();
    }

    /// <summary>
    /// Assegna una nave specifica a una banchina specifica.
    /// Calcola il primo giorno disponibile per l'attracco senza conflitti e aggiorna lo stato della nave in "Assigned".
    /// </summary>
    /// <param name="shipId">ID univoco della nave da assegnare.</param>
    /// <param name="berthId">ID univoco della banchina di destinazione.</param>
    /// <returns>Un DTO di risposta contenente i dettagli dell'assegnazione temporale calcolata.</returns>
    /// <exception cref="KeyNotFoundException">Lanciata se la nave o la banchina non esistono nel database.</exception>
    /// <exception cref="InvalidOperationException">Lanciata se la nave non è in attesa o se la taglia non è compatibile.</exception>
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

        // 4. Algoritmo: Trova il primo slot temporale disponibile a partire dal giorno di arrivo della nave
        int startDay = FindFirstAvailableSlot(berth, ship.GiornoArrivo, ship.DurataOccupazione);
        int endDay = startDay + ship.DurataOccupazione - 1;

        // 5. Aggiorna lo stato della nave a "Assigned"
        ship.Stato = "Assigned";

        // 6. Crea il record di occupazione sul database collegando nave e banchina
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

    /// <summary>
    /// Trova il primo slot temporale disponibile in cui la nave può stazionare senza sovrapporsi ad altre navi.
    /// </summary>
    /// <param name="berth">L'entità banchina con le sue occupazioni correnti caricate.</param>
    /// <param name="earliestStart">Il giorno minimo a partire dal quale calcolare lo slot (giorno di arrivo della nave).</param>
    /// <param name="duration">La durata del soggiorno in giorni.</param>
    /// <returns>Il giorno di inizio calcolato per lo slot libero.</returns>
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
                // Calcoliamo la fine dell'occupazione esistente: (Inizio + Durata - 1)
                int existingEnd = existing.GiornoInizio + (existing.Nave?.DurataOccupazione ?? 0) - 1;

                // Controllo intersezione di intervalli temporali: [candidateStart, candidateEnd] si sovrappone a [existing.GiornoInizio, existingEnd] ?
                if (candidateStart <= existingEnd && candidateEnd >= existing.GiornoInizio)
                {
                    hasConflict = true;
                    // Se c'è conflitto, sposta il candidato al giorno successivo al termine dell'occupazione esistente
                    candidateStart = existingEnd + 1;
                    break; // Esci dal ciclo per ripartire con il nuovo giorno candidato
                }
            }
        } while (hasConflict);

        return candidateStart;
    }
}
