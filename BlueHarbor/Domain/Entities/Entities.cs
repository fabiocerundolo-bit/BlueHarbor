namespace BlueHarbor.Domain.Entities;

using BlueHarbor.Domain.Enums;

public class Ruolo
{
    public int IdRuolo { get; set; }
    public string NomeRuolo { get; set; } = string.Empty;
}

public class Dimensione
{
    public int IdDimensione { get; set; }
    public string NomeDimensione { get; set; } = string.Empty;
}

public class Utente
{
    public int IdUtente { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRuolo { get; set; }
    public Ruolo Ruolo { get; set; } = null!;
}

public class Banchina
{
    public int IdBanchina { get; set; }
    public string NomeBanchina { get; set; } = string.Empty;
    public int IdDimensione { get; set; }
    public Dimensione Dimensione { get; set; } = null!;
    
    public ICollection<Occupazione> Occupazioni { get; set; } = new List<Occupazione>();
}

public class Nave
{
    public int IdNave { get; set; }
    public string NomeNave { get; set; } = string.Empty;
    public int GiornoArrivo { get; set; }
    public int DurataOccupazione { get; set; }
    public string Stato { get; set; } = "Pending"; // 'Pending', 'Assigned', 'Departed'
    public string? Note { get; set; }
    public int IdDimensione { get; set; }
    public Dimensione Dimensione { get; set; } = null!;
    public int IdUtente { get; set; }
    public Utente Utente { get; set; } = null!;
}

public class Occupazione
{
    public int IdOccupazione { get; set; }
    public int GiornoInizio { get; set; }
    public int IdNave { get; set; }
    public Nave Nave { get; set; } = null!;
    public int IdBanchina { get; set; }
    public Banchina Banchina { get; set; } = null!;
    public int IdUtente { get; set; }
    public Utente Utente { get; set; } = null!;
}

public class SystemState
{
    public int Id { get; set; } = 1;
    public int CurrentDay { get; set; } = 1;
}
