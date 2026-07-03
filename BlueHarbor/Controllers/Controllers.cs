using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Security;
using BlueHarbor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueHarbor.Controllers;

// ============================================================
// SHIPS CONTROLLER - Role: Operator
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Operator + "," + Roles.Scheduler)]
public class ShipsController(
    IShipService shipService,
    IShipRepository shipRepository,
    IListaNaviRepository listaNaviRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the ship templates available for creation.
    /// </summary>
    [HttpGet("ship-list")]
    public async Task<IActionResult> GetListaNavi()
    {
        var listaNavi = await listaNaviRepository.GetAllAsync();
        var result = listaNavi
            .Select(item => new ListaNaviDto(item.IdListaNavi, item.NomeNave, item.Dimensione.SizeName))
            .OrderBy(item => item.Name)
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Retrieves all ships registered in the system.
    /// Includes information about the assigned berth (if any).
    /// Accessible to both the Operator and the Scheduler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllShips()
    {
        // ✅ Calls the repository directly (not the service)
        // because GetAllShipsAsync is a simple query with no business logic
        var ships = await shipRepository.GetAllShipsAsync();
        return Ok(ships);
    }

    /// <summary>
    /// Registers a new ship in the system.
    /// Size, arrival day, and duration are generated automatically.
    /// </summary>
    /// <param name="request">Ship data (Name required, Notes optional)</param>
    /// <returns>The created ship with generated data</returns>
    /// <response code="201">Ship created successfully</response>
    /// <response code="400">Invalid input data (e.g. empty or too-short name)</response>
    [HttpPost]
    [Authorize(Roles = Roles.Operator)]
    public async Task<IActionResult> CreateShip([FromBody] CreateShipRequest request)
    {
        // ✅ Validation is handled automatically by [ApiController]
        // via Data Annotations on CreateShipRequest.
        // If ModelState is invalid, ASP.NET returns 400 automatically.
        
        try
        {
            var ship = await shipService.CreateShipAsync(request);
            return CreatedAtAction(nameof(GetShip), new { id = ship.Id }, ship);
        }
        catch (Exception)
        {
            return StatusCode(500, "An internal error occurred while creating the ship.");
        }
    }

    /// <summary>
    /// Retrieves the details of a single ship by ID.
    /// </summary>
    /// <param name="id">Ship ID</param>
    /// <returns>Ship details</returns>
    /// <response code="200">Ship found</response>
    /// <response code="404">Ship not found</response>
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Operator)]
    public async Task<IActionResult> GetShip(int id)
    {
        var ship = (await shipRepository.GetAllShipsAsync()).FirstOrDefault(s => s.ShipId == id);
        if (ship == null) return NotFound();
        return Ok(ship);
    }
}

// ============================================================
// SCHEDULER CONTROLLER - Role: Scheduler
// ============================================================
[ApiController]
[Route("api/scheduler")]
[Authorize(Roles = Roles.Scheduler)]
public class SchedulerController(ISchedulerService schedulerService) : ControllerBase
{
    /// <summary>
    /// Retrieves the list of all berths with their respective occupancies.
    /// </summary>
    [HttpGet("berths")]
    public async Task<IActionResult> GetBerths()
    {
        var berths = await schedulerService.GetBerthsAsync();
        return Ok(berths);
    }

    /// <summary>
    /// Retrieves the list of ships in "Pending" status waiting for assignment.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingShips()
    {
        var ships = await schedulerService.GetPendingShipsAsync();
        return Ok(ships);
    }

    /// <summary>
    /// Assigns a ship to a specific berth.
    /// Automatically calculates the first available time slot.
    /// </summary>
    /// <param name="request">ShipId and BerthId</param>
    /// <returns>Assignment details (start/end days, new status)</returns>
    /// <response code="200">Assignment completed</response>
    /// <response code="400">Incompatible size or ship not in Pending status</response>
    /// <response code="404">Ship or berth not found</response>
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
            return StatusCode(500, "Internal error during assignment.");
        }
    }
}

// ============================================================
// SYSTEM CONTROLLER - Role: Both (Operator and Scheduler)
// ============================================================
[ApiController]
[Route("api/system")]
[Authorize]
public class SystemController(
    ITimeManagementService timeManagementService,
    ISystemStateRepository systemStateRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the current virtual day of the system.
    /// </summary>
    [HttpGet("day")]
    public async Task<IActionResult> GetCurrentDay()
    {
        var state = await systemStateRepository.GetAsync();
        return Ok(new { currentDay = state.CurrentDay });
    }

    /// <summary>
    /// Advances the virtual day by 1 unit.
    /// Triggers a background Hangfire job to update departed ships.
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
            return StatusCode(500, "Internal error while advancing the day.");
        }
    }
}