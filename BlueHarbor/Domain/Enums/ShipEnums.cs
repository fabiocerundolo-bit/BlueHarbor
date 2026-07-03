namespace BlueHarbor.Domain.Enums;

public enum ShipSize
{
    XL,
    L,
    M,
    S
}

public enum ShipStatus
{
    Pending,    // Waiting for assignment
    Assigned,   // Berth assigned
    Departed    // Occupancy ended
}
