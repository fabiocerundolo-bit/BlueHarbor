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
    public async Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request)
    {
        var state = await stateRepository.GetAsync();
        
        var sizes = Enum.GetValues<ShipSize>();
        var ship = new Ship
        {
            Name = request.Name.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Size = sizes[Random.Shared.Next(sizes.Length)],
            ArrivalDay = state.CurrentDay + Random.Shared.Next(1, 31),
            DurationDays = Random.Shared.Next(3, 16),
            Status = ShipStatus.Pending
        };

        await shipRepository.AddAsync(ship);
        
        return new ShipResponseDto(
            ship.Id,
            ship.Name,
            ship.Notes,
            ship.Size,
            ship.ArrivalDay,
            ship.DurationDays,
            ship.Status
        );
    }

    public async Task<AssignmentDto> AssignBerthAsync(int shipId, int berthId)
    {
        var ship = await shipRepository.GetByIdAsync(shipId) ?? throw new Exception("Ship not found");
        if (ship.Status != ShipStatus.Pending) throw new InvalidOperationException("Ship not pending");

        var berth = await berthRepository.GetByIdAsync(berthId) ?? throw new Exception("Berth not found");
        if (berth.Size != ship.Size) throw new InvalidOperationException("Size mismatch");

        int proposedStartDay = FindFirstAvailableSlot(berth, ship.ArrivalDay, ship.DurationDays);
        
        ship.Status = ShipStatus.Assigned;
        ship.AssignedBerthId = berth.Id;
        ship.StartDay = proposedStartDay;

        // Aggiungiamo l'assegnazione alla collezione della banchina per permettere a EF di tracciarla
        // e per far sì che FindFirstAvailableSlot funzioni se riutilizziamo lo stesso oggetto Berth
        var newAssignment = new Assignment
        {
            ShipId = ship.Id,
            BerthId = berth.Id,
            StartDay = proposedStartDay,
            EndDay = proposedStartDay + ship.DurationDays - 1
        };
        berth.Assignments.Add(newAssignment);

        await shipRepository.UpdateAsync(ship);
        
        return new AssignmentDto(ship.Id, berth.Id, proposedStartDay, newAssignment.EndDay);
    }

    private int FindFirstAvailableSlot(Berth berth, int earliestStart, int duration)
    {
        int candidateDay = earliestStart;
        bool conflict;

        do
        {
            conflict = false;
            int candidateEnd = candidateDay + duration - 1;

            foreach (var existing in berth.Assignments)
            {
                if (candidateDay <= existing.EndDay && candidateEnd >= existing.StartDay)
                {
                    conflict = true;
                    candidateDay = existing.EndDay + 1;
                    break;
                }
            }
        } while (conflict);

        return candidateDay;
    }
}
