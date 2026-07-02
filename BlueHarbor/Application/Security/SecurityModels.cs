namespace BlueHarbor.Application.Security;

/// <summary>
/// Mock database containing users enabled to access the system and their respective roles.
/// Used to simulate authentication via HTTP headers.
/// </summary>
public static class MockUserDatabase
{
    // Map of mock users and their respective role authorizations
    private static readonly Dictionary<string, string> Users = new()
    {
        { "operator1", Roles.Operator },
        { "operator2", Roles.Operator },
        { "scheduler1", Roles.Scheduler },
        { "scheduler2", Roles.Scheduler }
    };

    /// <summary>
    /// Retrieves the role associated with a given username.
    /// </summary>
    public static string? GetRole(string username) 
        => Users.TryGetValue(username, out var role) ? role : null;

    /// <summary>
    /// Returns the list of all valid usernames in the mock system.
    /// </summary>
    public static IEnumerable<string> GetValidUsernames() 
        => Users.Keys;
}

/// <summary>
/// Definition of constants for the security roles supported by the application.
/// </summary>
public static class Roles
{
    public const string Operator = "Operator";
    public const string Scheduler = "Scheduler";
}