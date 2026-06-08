using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Application.DTOs;

public record CreateShipRequest(string Name, string? Notes);

public record ShipDto(int Id, string Name, string? Notes, ShipSize Size, int ArrivalDay, int DurationDays, ShipStatus Status, int? StartDay);

public record ShipResponseDto(
    int Id,
    string Name,
    string? Notes,
    ShipSize Size,
    int ArrivalDay,
    int DurationDays,
    ShipStatus Status
);

public record PendingShipDto(
    int Id,
    string Name,
    ShipSize Size,
    int ArrivalDay,
    int DurationDays
);

public record AssignShipRequest(int ShipId, int BerthId);

public record AssignmentResponseDto(
    int ShipId,
    int BerthId,
    int StartDay,
    int EndDay,
    ShipStatus NewStatus
);

public record AssignmentDto(int ShipId, int BerthId, int StartDay, int EndDay);

public record NextDayResponseDto(int NewCurrentDay);
