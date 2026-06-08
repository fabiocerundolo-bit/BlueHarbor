using System.Text.Json.Serialization;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Application.DTOs;

public record CreateShipRequest(string Name, string? Notes);

public record ShipDto(
    [property: JsonPropertyName("id")] int IdNave,
    [property: JsonPropertyName("name")] string NomeNave,
    [property: JsonPropertyName("notes")] string? Note,
    [property: JsonPropertyName("size")] string NomeDimensione,
    [property: JsonPropertyName("arrivalDay")] int GiornoArrivo,
    [property: JsonPropertyName("durationDays")] int DurataOccupazione,
    [property: JsonPropertyName("status")] string Stato,
    [property: JsonPropertyName("startDay")] int? GiornoInizio,
    [property: JsonPropertyName("assignedBerthId")] int? IdBanchina,
    [property: JsonPropertyName("assignedBerthName")] string? NomeBanchina
);

public record ShipResponseDto(
    int Id,
    string Name,
    string? Notes,
    string Size,
    int ArrivalDay,
    int DurationDays,
    string Status
);

public record PendingShipDto(
    int Id,
    string Name,
    string Size,
    int ArrivalDay,
    int DurationDays
);

public record AssignShipRequest(int ShipId, int BerthId);

public record AssignmentResponseDto(
    int ShipId,
    int BerthId,
    int StartDay,
    int EndDay,
    string NewStatus
);

public record AssignmentDto(int ShipId, int BerthId, int StartDay, int EndDay);

public record BerthDto(int Id, string Name, string Size, IEnumerable<BerthAssignmentDto> Assignments);

public record BerthAssignmentDto(int Id, int ShipId, string ShipName, int StartDay, int EndDay, string Status);

public record NextDayResponseDto(int NewCurrentDay);
