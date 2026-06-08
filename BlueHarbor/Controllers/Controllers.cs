using BlueHarbor.Application.DTOs;
using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Security;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlueHarbor.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Operatore)]
public class ShipsController(IShipService shipService, IShipRepository shipRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllShips()
    {
        var ships = await shipRepository.GetAllShipsAsync(); // ← Repository, non Service
        return Ok(ships);
    }

    [HttpPost]
    public async Task<IActionResult> CreateShip([FromBody] CreateShipRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Il nome della nave è obbligatorio.");
        }

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShip(int id)
    {
        var ship = await shipRepository.GetByIdAsync(id);
        if (ship == null) return NotFound();
        return Ok(ship);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Scheduler)]
public class SchedulerController(ISchedulerService schedulerService) : ControllerBase
{
    [HttpGet("berths")]
    public async Task<IActionResult> GetBerths()
    {
        var berths = await schedulerService.GetBerthsAsync();
        return Ok(berths);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingShips()
    {
        var ships = await schedulerService.GetPendingShipsAsync();
        return Ok(ships);
    }

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

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SystemController(
    ITimeManagementService timeManagementService,
    ISystemStateRepository systemStateRepository) : ControllerBase
{
    [HttpGet("day")]
    public async Task<IActionResult> GetCurrentDay()
    {
        var state = await systemStateRepository.GetAsync();
        return Ok(new { currentDay = state.CurrentDay });
    }

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
