using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Servizio responsabile della registrazione delle navi nel sistema porto.
/// Gestisce la logica di assegnamento casuale delle caratteristiche per simulare il traffico marittimo.
/// </summary>
public class ShipService(
    IShipRepository shipRepository,
    ISystemStateRepository stateRepository) : IShipService
{
    /// <summary>
    /// Registra una nuova nave nel sistema generando in maniera pseudo-casuale la sua dimensione,
    /// il giorno previsto di arrivo e la durata della permanenza.
    /// </summary>
    /// <param name="request">I dati base della nave (Nome e Note).</param>
    /// <returns>I dettagli della nave creata.</returns>
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
            // Giorno di arrivo pianificato tra domani (giorno corrente + 1) e i successivi 30 giorni
            GiornoArrivo = state.CurrentDay + Random.Shared.Next(1, 31),
            // Durata dell'occupazione compresa tra 3 e 15 giorni
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