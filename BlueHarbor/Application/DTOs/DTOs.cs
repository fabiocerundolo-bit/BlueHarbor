using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Application.DTOs;

public record CreateShipRequest(
    [Required(ErrorMessage = "Il nome della nave è obbligatorio.")]
    [MinLength(3, ErrorMessage = "Il nome deve avere almeno 3 caratteri.")]
    [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    string Name, 
    [MaxLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri.")]
    string? Notes);

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

public record AssignShipRequest(
    [Range(1, int.MaxValue, ErrorMessage ="L'ID della nave non è valido.")]
    int ShipId, 
    [Range(1, int.MaxValue, ErrorMessage ="L'ID della banchina non è valido.")]
    int BerthId
    );

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
