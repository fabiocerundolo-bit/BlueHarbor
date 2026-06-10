using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

public class ShipService(
    IShipRepository shipRepository,
    ISystemStateRepository stateRepository) : IShipService
{
    public async Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request)
    {
        var state = await stateRepository.GetAsync();
        
        // Assegniamo una dimensione casuale (1=XL, 2=L, 3=M, 4=S)
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
            IdUtente = 1 // Admin di default
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
}