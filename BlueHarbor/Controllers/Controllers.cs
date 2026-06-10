using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Security;
using BlueHarbor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueHarbor.Controllers;

// ============================================================
// SHIPS CONTROLLER - Ruolo: Operatore
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Operatore)]
public class ShipsController(IShipService shipService, IShipRepository shipRepository) : ControllerBase
{
    /// <summary>
    /// Recupera tutte le navi registrate nel sistema.
    /// Include informazioni sulla banchina assegnata (se presente).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllShips()
    {
        // ✅ Chiama direttamente il repository (non il service)
        // perché GetAllShipsAsync è una query semplice senza logica di business
        var ships = await shipRepository.GetAllShipsAsync();
        return Ok(ships);
    }

    /// <summary>
    /// Registra una nuova nave nel sistema.
    /// Dimensione, giorno di arrivo e durata vengono generati automaticamente.
    /// </summary>
    /// <param name="request">Dati della nave (Name obbligatorio, Notes opzionale)</param>
    /// <returns>La nave creata con i dati generati</returns>
    /// <response code="201">Nave creata con successo</response>
    /// <response code="400">Dati di input non validi (es. nome vuoto o troppo corto)</response>
    [HttpPost]
    public async Task<IActionResult> CreateShip([FromBody] CreateShipRequest request)
    {
        // ✅ La validazione è gestita automaticamente da [ApiController]
        // tramite le Data Annotations su CreateShipRequest.
        // Se il ModelState è invalido, ASP.NET restituisce 400 automaticamente.
        
        try
        {
            var ship = await shipService.CreateShipAsync(request);
            return CreatedAtAction(nameof(GetShip), new { id = ship.Id }, ship);
        }
        catch (Exception)
        {
            return StatusCode(500, "Si è verificato un errore interno durante la creazione della nave.");
        }
    }

    /// <summary>
    /// Recupera i dettagli di una singola nave tramite ID.
    /// </summary>
    /// <param name="id">ID della nave</param>
    /// <returns>Dettagli della nave</returns>
    /// <response code="200">Nave trovata</response>
    /// <response code="404">Nave non trovata</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShip(int id)
    {
        var ship = await shipRepository.GetByIdAsync(id);
        if (ship == null) return NotFound();
        return Ok(ship);
    }
}

// ============================================================
// SCHEDULER CONTROLLER - Ruolo: Scheduler
// ============================================================
[ApiController]
[Route("api/scheduler")]
[Authorize(Roles = Roles.Scheduler)]
public class SchedulerController(ISchedulerService schedulerService) : ControllerBase
{
    /// <summary>
    /// Recupera l'elenco di tutte le banchine con le relative occupazioni.
    /// </summary>
    [HttpGet("berths")]
    public async Task<IActionResult> GetBerths()
    {
        var berths = await schedulerService.GetBerthsAsync();
        return Ok(berths);
    }

    /// <summary>
    /// Recupera l'elenco delle navi in stato "Pending" da assegnare.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingShips()
    {
        var ships = await schedulerService.GetPendingShipsAsync();
        return Ok(ships);
    }

    /// <summary>
    /// Assegna una nave a una banchina specifica.
    /// Calcola automaticamente il primo slot temporale disponibile.
    /// </summary>
    /// <param name="request">ShipId e BerthId</param>
    /// <returns>Dettagli dell'assegnazione (giorni di inizio/fine, nuovo stato)</returns>
    /// <response code="200">Assegnazione completata</response>
    /// <response code="400">Dimensione incompatibile o nave non in stato Pending</response>
    /// <response code="404">Nave o banchina non trovata</response>
    [HttpPost("assign")]
    public async Task<IActionResult> AssignShip([FromBody] AssignShipRequest request)
    {
        try
        {
            var result = await schedulerService.AssignShipToBerthAsync(request.ShipId, request.BerthId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Errore interno durante l'assegnazione.");
        }
    }
}

// ============================================================
// SYSTEM CONTROLLER - Ruolo: Entrambi (Operatore e Scheduler)
// ============================================================
[ApiController]
[Route("api/system")]
[Authorize]
public class SystemController(
    ITimeManagementService timeManagementService,
    ISystemStateRepository systemStateRepository) : ControllerBase
{
    /// <summary>
    /// Recupera il giorno virtuale corrente del sistema.
    /// </summary>
    [HttpGet("day")]
    public async Task<IActionResult> GetCurrentDay()
    {
        var state = await systemStateRepository.GetAsync();
        return Ok(new { currentDay = state.CurrentDay });
    }

    /// <summary>
    /// Avanza il giorno virtuale di 1 unità.
    /// Attiva in background il job Hangfire per aggiornare le navi partite.
    /// </summary>
    [HttpPost("next-day")]
    public async Task<IActionResult> NextDay()
    {
        try
        {
            var nextDay = await timeManagementService.AdvanceDayAsync();
            return Ok(new NextDayResponseDto(nextDay));
        }
        catch (Exception)
        {
            return StatusCode(500, "Errore interno durante l'avanzamento del giorno.");
        }
    }
}