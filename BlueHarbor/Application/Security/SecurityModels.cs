namespace BlueHarbor.Application.Security;

/// <summary>
/// Database mock contenente gli utenti abilitati all'accesso del sistema e i rispettivi ruoli.
/// Utilizzato per simulare l'autenticazione tramite intestazioni HTTP.
/// </summary>
public static class MockUserDatabase
{
    // Mappa degli utenti fittizi e delle rispettive autorizzazioni di ruolo
    private static readonly Dictionary<string, string> Users = new()
    {
        { "operatore1", Roles.Operatore },
        { "operatore2", Roles.Operatore },
        { "scheduler1", Roles.Scheduler },
        { "scheduler2", Roles.Scheduler }
    };

    /// <summary>
    /// Recupera il ruolo associato a un determinato username.
    /// </summary>
    public static string? GetRole(string username) 
        => Users.TryGetValue(username, out var role) ? role : null;

    /// <summary>
    /// Restituisce la lista di tutti gli username validi nel sistema mock.
    /// </summary>
    public static IEnumerable<string> GetValidUsernames() 
        => Users.Keys;
}

/// <summary>
/// Definizione delle costanti per i ruoli di sicurezza supportati dall'applicazione.
/// </summary>
public static class Roles
{
    public const string Operatore = "Operatore";
    public const string Scheduler = "Scheduler";
}