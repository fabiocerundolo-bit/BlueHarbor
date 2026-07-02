using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Service responsible for registering ships in the harbor system.
/// Handles the logic for randomly assigning ship characteristics to simulate maritime traffic.
/// </summary>
public class ShipService(
    IShipRepository shipRepository,
    ISystemStateRepository stateRepository) : IShipService
{
    /// <summary>
    /// Registers a new ship in the system by pseudo-randomly generating its size,
    /// expected arrival day, and duration of stay.
    /// </summary>
    /// <param name="request">Basic ship data (Name and Notes).</param>
    /// <returns>Details of the created ship.</returns>
    public async Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request)
    {
        var state = await stateRepository.GetAsync();
        
        // Assign a random size (1=XL, 2=L, 3=M, 4=S)
        int randomSizeId = Random.Shared.Next(1, 5);
        string[] sizeNames = ["XL", "L", "M", "S"];
        
        var ship = new Ship
        {
            ShipName = request.Name.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            SizeId = randomSizeId,
            // Planned arrival day between tomorrow (current day + 1) and the next 30 days
            ArrivalDay = state.CurrentDay + Random.Shared.Next(1, 31),
            // Occupancy duration between 3 and 15 days
            DurationDays = Random.Shared.Next(3, 16),
            Status = "Pending",
            UserId = 1 // Default Admin
        };

        await shipRepository.AddAsync(ship);
        
        return new ShipResponseDto(
            ship.ShipId,
            ship.ShipName,
            ship.Notes,
            sizeNames[randomSizeId - 1],
            ship.ArrivalDay,
            ship.DurationDays,
            ship.Status
        );
    }
}