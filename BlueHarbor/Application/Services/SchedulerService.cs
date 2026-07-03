using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Service responsible for managing and scheduling ships across harbor berths.
/// Contains the business logic for verifying size compatibility and allocating non-overlapping time slots.
/// </summary>
public class SchedulerService(IShipRepository shipRepository, IBerthRepository berthRepository) : ISchedulerService
{
    /// <summary>
    /// Retrieves all ships waiting to be scheduled (in "Pending" status).
    /// </summary>
    /// <returns>A list of DTOs containing information about pending ships.</returns>
    public async Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync()
    {
        return await shipRepository.GetPendingShipsAsync();
    }

    /// <summary>
    /// Retrieves all available berths along with their scheduled occupancy details.
    /// </summary>
    /// <returns>A list of DTOs representing berths and their respective assignments.</returns>
    public async Task<IEnumerable<BerthDto>> GetBerthsAsync()
    {
        return await berthRepository.GetBerthsWithAssignmentsAsync();
    }

    /// <summary>
    /// Assigns a specific ship to a specific berth.
    /// Calculates the first available time slot for docking without conflicts and updates the ship status to "Assigned".
    /// </summary>
    /// <param name="shipId">Unique ID of the ship to assign.</param>
    /// <param name="berthId">Unique ID of the target berth.</param>
    /// <returns>A response DTO containing the details of the calculated time assignment.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the ship or berth does not exist in the database.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the ship is not in Pending status or the size is incompatible.</exception>
    public async Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId)
    {
        // 1. Retrieve the ship and verify it is in Pending status
        var ship = await shipRepository.GetByIdAsync(shipId);
        if (ship == null) throw new KeyNotFoundException("Ship not found.");
        if (ship.Status != "Pending") 
            throw new InvalidOperationException("The ship is not in Pending status.");

        // 2. Retrieve the berth with its existing assignments
        var berth = await berthRepository.GetByIdAsync(berthId);
        if (berth == null) throw new KeyNotFoundException("Berth not found.");

        // 3. Domain rule: the berth must be compatible by size
        if (berth.SizeId != ship.ListaNavi.FK_Id_Dimensione)
        {
            throw new InvalidOperationException($"Incompatible size. Ship: {ship.ListaNavi.FK_Id_Dimensione}, Berth: {berth.SizeId}");
        }

        // 4. Algorithm: Find the first available time slot starting from the ship's arrival day
        int startDay = FindFirstAvailableSlot(berth, ship.ArrivalDay, ship.DurationDays);
        int endDay = startDay + ship.DurationDays - 1;

        // 5. Update the ship's status to "Assigned"
        ship.Status = "Assigned";

        // 6. Create the occupancy record and save both changes in a single transaction
        var occupancy = new Occupancy
        {
            ShipId = ship.ShipId,
            BerthId = berth.BerthId,
            StartDay = startDay,
            UserId = 1 // Default Admin
        };

        await shipRepository.AddAssignmentAndUpdateShipAsync(occupancy, ship);

        return new AssignmentResponseDto(ship.ShipId, berth.BerthId, startDay, endDay, ship.Status);
    }

    /// <summary>
    /// Finds the first available time slot in which the ship can stay without overlapping with other ships.
    /// </summary>
    /// <param name="berth">The berth entity with its current occupancies loaded.</param>
    /// <param name="earliestStart">The minimum day from which to calculate the slot (ship's arrival day).</param>
    /// <param name="duration">The duration of the stay in days.</param>
    /// <returns>The calculated start day for the free slot.</returns>
    private int FindFirstAvailableSlot(Berth berth, int earliestStart, int duration)
    {
        int candidateStart = earliestStart;
        bool hasConflict;

        do
        {
            hasConflict = false;
            int candidateEnd = candidateStart + duration - 1;

            foreach (var existing in berth.Occupancies)
            {
                // Calculate the end of the existing occupancy: (StartDay + DurationDays - 1)
                int existingEnd = existing.StartDay + (existing.Ship?.DurationDays ?? 0) - 1;

                // Check time interval intersection: does [candidateStart, candidateEnd] overlap [existing.StartDay, existingEnd]?
                if (candidateStart <= existingEnd && candidateEnd >= existing.StartDay)
                {
                    hasConflict = true;
                    // If there is a conflict, move the candidate to the day after the existing occupancy ends
                    candidateStart = existingEnd + 1;
                    break; // Exit the loop to restart with the new candidate day
                }
            }
        } while (hasConflict);

        return candidateStart;
    }
}
