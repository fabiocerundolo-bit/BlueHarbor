using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Application.DTOs;

/// <summary>
/// DTO di richiesta per la creazione di una nuova nave da parte dell'Operatore.
/// </summary>
public record CreateShipRequest(
    [Required(ErrorMessage = "Il nome della nave è obbligatorio.")]
    [MinLength(3, ErrorMessage = "Il nome deve avere almeno 3 caratteri.")]
    [MaxLength(100, ErrorMessage = "Il nome non può superare i 100 caratteri.")]
    string Name, 
    [MaxLength(500, ErrorMessage = "Le note non possono superare i 500 caratteri.")]
    string? Notes);

/// <summary>
/// Rappresentazione dettagliata di una nave, incluse le informazioni di sosta ed eventuale banchina assegnata.
/// </summary>
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

/// <summary>
/// DTO restituito in seguito alla creazione corretta di una nave.
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
/// Rappresenta una nave in attesa di pianificazione (in stato "Pending").
/// </summary>
public record PendingShipDto(
    int Id,
    string Name,
    string Size,
    int ArrivalDay,
    int DurationDays
);

/// <summary>
/// DTO di richiesta per assegnare una nave ad una banchina specifica.
/// </summary>
public record AssignShipRequest(
    [Range(1, int.MaxValue, ErrorMessage ="L'ID della nave non è valido.")]
    int ShipId, 
    [Range(1, int.MaxValue, ErrorMessage ="L'ID della banchina non è valido.")]
    int BerthId
    );

/// <summary>
/// Risposta ad un'operazione di pianificazione completata con successo.
/// </summary>
public record AssignmentResponseDto(
    int ShipId,
    int BerthId,
    int StartDay,
    int EndDay,
    string NewStatus
);

/// <summary>
/// DTO che sintetizza un singolo record di occupazione banchina.
/// </summary>
public record AssignmentDto(int ShipId, int BerthId, int StartDay, int EndDay);

/// <summary>
/// DTO per rappresentare una banchina e l'insieme delle sue prenotazioni/occupazioni temporali.
/// </summary>
public record BerthDto(int Id, string Name, string Size, IEnumerable<BerthAssignmentDto> Assignments);

/// <summary>
/// DTO che descrive una specifica assegnazione per una banchina.
/// </summary>
public record BerthAssignmentDto(int Id, int ShipId, string ShipName, int StartDay, int EndDay, string Status);

/// <summary>
/// Risposta restituita al completamento dell'avanzamento del giorno virtuale di sistema.
/// </summary>
public record NextDayResponseDto(int NewCurrentDay);

