using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Application.DTOs;

/// <summary>
/// Request DTO for creating a new ship by the Operator.
/// </summary>
public record CreateShipRequest(
    [Required(ErrorMessage = "The ship list ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The ship list ID is not valid.")]
    int IdListaNavi,
    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    string? Notes,
    [MaxLength(200, ErrorMessage = "Ship name cannot exceed 200 characters.")]
    string? CustomName = null);

/// <summary>
/// Detailed representation of a ship, including docking information and the assigned berth (if any).
/// </summary>
public record ShipDto(
    [property: JsonPropertyName("id")] int ShipId,
    [property: JsonPropertyName("name")] string ShipName,
    [property: JsonPropertyName("notes")] string? Notes,
    [property: JsonPropertyName("size")] string SizeName,
    [property: JsonPropertyName("arrivalDay")] int ArrivalDay,
    [property: JsonPropertyName("durationDays")] int DurationDays,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("startDay")] int? StartDay,
    [property: JsonPropertyName("assignedBerthId")] int? BerthId,
    [property: JsonPropertyName("assignedBerthName")] string? BerthName
);

/// <summary>
/// DTO returned after a ship has been successfully created.
/// </summary>
public record ShipResponseDto(
    int Id,
    string Name,
    string? Notes,
    string Size,
    int ArrivalDay,
    int DurationDays,
    string Status
);

/// <summary>
/// Lightweight DTO describing a ship template available for creation.
/// </summary>
public record ListaNaviDto(
    int Id,
    string Name,
    string Size
);

/// <summary>
/// Represents a ship waiting for scheduling (in "Pending" status).
/// </summary>
public record PendingShipDto(
    int Id,
    string Name,
    string Size,
    int ArrivalDay,
    int DurationDays
);

/// <summary>
/// Request DTO for assigning a ship to a specific berth.
/// </summary>
public record AssignShipRequest(
    [Range(1, int.MaxValue, ErrorMessage = "The ship ID is not valid.")]
    int ShipId, 
    [Range(1, int.MaxValue, ErrorMessage = "The berth ID is not valid.")]
    int BerthId
    );

/// <summary>
/// Response for a successfully completed scheduling operation.
/// </summary>
public record AssignmentResponseDto(
    int ShipId,
    int BerthId,
    int StartDay,
    int EndDay,
    string NewStatus
);

/// <summary>
/// DTO summarising a single berth occupancy record.
/// </summary>
public record AssignmentDto(int ShipId, int BerthId, int StartDay, int EndDay);

/// <summary>
/// DTO representing a berth and the set of its temporal occupancies.
/// </summary>
public record BerthDto(int Id, string Name, string Size, IEnumerable<BerthAssignmentDto> Assignments);

/// <summary>
/// DTO describing a specific assignment for a berth.
/// </summary>
public record BerthAssignmentDto(int Id, int ShipId, string ShipName, int StartDay, int EndDay, string Status);

/// <summary>
/// Response returned when the virtual system day has been successfully advanced.
/// </summary>
public record NextDayResponseDto(int NewCurrentDay);
