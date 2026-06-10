namespace BlueHarbor.Domain.Entities;

using BlueHarbor.Domain.Enums;

/// <summary>
/// Rappresenta il ruolo di sicurezza associato ad un utente (es. Operatore, Scheduler).
/// </summary>
public class Ruolo
{
    public int IdRuolo { get; set; }
    public string NomeRuolo { get; set; } = string.Empty;
}

/// <summary>
/// Rappresenta la taglia/dimensione di una nave o di una banchina (es. XL, L, M, S).
/// </summary>
public class Dimensione
{
    public int IdDimensione { get; set; }
    public string NomeDimensione { get; set; } = string.Empty;
}

/// <summary>
/// Rappresenta un utente registrato che può eseguire operazioni nel sistema portuale.
/// </summary>
public class Utente
{
    public int IdUtente { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int IdRuolo { get; set; }
    public Ruolo Ruolo { get; set; } = null!;
}

/// <summary>
/// Rappresenta una banchina fisica del porto predisposta per l'attracco di navi di una specifica dimensione.
/// </summary>
public class Banchina
{
    public int IdBanchina { get; set; }
    public string NomeBanchina { get; set; } = string.Empty;
    public int IdDimensione { get; set; }
    public Dimensione Dimensione { get; set; } = null!;
    
    // Lista di tutte le occupazioni temporali associate a questa banchina
    public ICollection<Occupazione> Occupazioni { get; set; } = new List<Occupazione>();
}

/// <summary>
/// Rappresenta una nave registrata nell'applicazione con le sue preferenze di sosta ed il suo stato corrente.
/// </summary>
public class Nave
{
    public int IdNave { get; set; }
    public string NomeNave { get; set; } = string.Empty;
    public int GiornoArrivo { get; set; }
    public int DurataOccupazione { get; set; }
    
    // Stato corrente della nave: 'Pending' (in attesa), 'Assigned' (assegnata), 'Departed' (salpata)
    public string Stato { get; set; } = "Pending"; 
    public string? Note { get; set; }
    public int IdDimensione { get; set; }
    public Dimensione Dimensione { get; set; } = null!;
    public int IdUtente { get; set; }
    public Utente Utente { get; set; } = null!;
}

/// <summary>
/// Rappresenta l'occupazione temporale di una banchina da parte di una nave a partire da un determinato giorno d'inizio.
/// </summary>
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

/// <summary>
/// Rappresenta lo stato globale del sistema, tra cui il giorno virtuale corrente.
/// Gestito come un'entità singleton (singola riga con ID=1).
/// </summary>
public class SystemState
{
    public int Id { get; set; } = 1;
    public int CurrentDay { get; set; } = 1;
}

