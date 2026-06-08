namespace BlueHarbor.Domain.Entities;

using BlueHarbor.Domain.Enums;

public class Ship
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Regola: Dimensione assegnata automaticamente dal sistema
    public ShipSize Size { get; set; }
    
    // Regola: Giorno di arrivo casuale (CurrentDay + 1..30)
    public int ArrivalDay { get; set; }
    
    // Regola: Durata casuale tra 3 e 15 giorni
    public int DurationDays { get; set; }
    
    public ShipStatus Status { get; set; } = ShipStatus.Pending;
    
    // Relazioni opzionali, popolate solo dopo l'assegnazione
    public int? AssignedBerthId { get; set; }
    public Berth? AssignedBerth { get; set; }
    
    public int? StartDay { get; set; } // Giorno effettivo di inizio occupazione
}

public class Berth
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Es: "Berth-XL-1"
    public ShipSize Size { get; set; }
    
    // Una banchina può avere multiple assegnazioni nel tempo
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public class Assignment
{
    public int Id { get; set; }
    public int ShipId { get; set; }
    public Ship Ship { get; set; } = null!;
    
    public int BerthId { get; set; }
    public Berth Berth { get; set; } = null!;
    
    public int StartDay { get; set; }
    public int EndDay { get; set; } // Calcolato come: StartDay + DurationDays - 1
}

// Entità Singleton per gestire il modello temporale virtuale
public class SystemState
{
    public int Id { get; set; } = 1; // Chiave fissa per avere un solo record
    public int CurrentDay { get; set; } = 1; // Il sistema inizia dal Giorno 1
}
