using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

public class ShipService(
    IShipRepository shipRepository,
    IBerthRepository berthRepository,
    ISystemStateRepository stateRepository) : IShipService
{
    public async Task<IEnumerable<ShipDto>> GetAllShipsAsync()
    {
        // Questo metodo non è più usato direttamente dal controller,
        // ma lo lasciamo per coerenza con l'interfaccia se necessario.
        return await shipRepository.GetAllShipsAsync();
    }

    public async Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request)
    {
        var state = await stateRepository.GetAsync();
        
        // Assegniamo una dimensione casuale e un utente di default (IdUtente = 1)
        int randomDimId = Random.Shared.Next(1, 5);
        string[] dimNames = ["XL", "L", "M", "S"];
        
        var ship = new Nave
        {
            NomeNave = request.Name.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IdDimensione = randomDimId,
            GiornoArrivo = state.CurrentDay + Random.Shared.Next(1, 31),
            DurataOccupazione = Random.Shared.Next(3, 16),
            Stato = "Pending",
            IdUtente = 1 // Admin di default per ora
        };

        await shipRepository.AddAsync(ship);
        
        return new ShipResponseDto(
            ship.IdNave,
            ship.NomeNave,
            ship.Note,
            dimNames[randomDimId - 1],
            ship.GiornoArrivo,
            ship.DurataOccupazione,
            ship.Stato
        );
    }

    public async Task<AssignmentResponseDto> AssignBerthAsync(int shipId, int berthId)
    {
        var ship = await shipRepository.GetByIdAsync(shipId);
        if (ship == null) throw new KeyNotFoundException("Ship not found");
        if (ship.Stato != "Pending") throw new InvalidOperationException("Ship not pending");

        var berth = await berthRepository.GetByIdAsync(berthId);
        if (berth == null) throw new KeyNotFoundException("Berth not found");

        if (berth.IdDimensione != ship.IdDimensione)
            throw new InvalidOperationException("Size mismatch");

        int startDay = FindFirstAvailableSlot(berth, ship.GiornoArrivo, ship.DurataOccupazione);
        int endDay = startDay + ship.DurataOccupazione - 1;

        var assignment = new Occupazione
        {
            IdNave = ship.IdNave,
            IdBanchina = berth.IdBanchina,
            GiornoInizio = startDay,
            IdUtente = 1
        };

        ship.Stato = "Assigned";

        await shipRepository.AddAssignmentAsync(assignment);
        await shipRepository.UpdateAsync(ship);

        return new AssignmentResponseDto(ship.IdNave, berth.IdBanchina, startDay, endDay, ship.Stato);
    }

    private int FindFirstAvailableSlot(Banchina berth, int earliestStart, int duration)
    {
        int candidateDay = earliestStart;
        bool conflict;

        do
        {
            conflict = false;
            int candidateEnd = candidateDay + duration - 1;

            foreach (var existing in berth.Occupazioni)
            {
                int existingEnd = existing.GiornoInizio + (existing.Nave?.DurataOccupazione ?? 0) - 1;
                if (candidateDay <= existingEnd && candidateEnd >= existing.GiornoInizio)
                {
                    conflict = true;
                    candidateDay = existingEnd + 1;
                    break;
                }
            }
        } while (conflict);

        return candidateDay;
    }
}
