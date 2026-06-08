namespace BlueHarbor.Application.Security;

public static class MockUserDatabase
{
    private static readonly Dictionary<string, string> Users = new()
    {
        { "operatore1", Roles.Operatore },
        { "operatore2", Roles.Operatore },
        { "scheduler1", Roles.Scheduler },
        { "scheduler2", Roles.Scheduler }
    };

    public static string? GetRole(string username) 
        => Users.TryGetValue(username, out var role) ? role : null;

    public static IEnumerable<string> GetValidUsernames() 
        => Users.Keys;
}

public static class Roles
{
    public const string Operatore = "Operatore";
    public const string Scheduler = "Scheduler";
}