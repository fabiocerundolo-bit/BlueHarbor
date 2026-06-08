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
    Pending,    // In attesa di assegnazione
    Assigned,   // Banchina assegnata
    Departed    // Occupazione terminata
}
