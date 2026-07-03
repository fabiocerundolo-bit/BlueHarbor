namespace BlueHarbor.Security;

using BlueHarbor.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

/// <summary>
/// Mock authentication handler to simulate user access.
/// Reads the custom "X-Username" header to identify the user and their role from the mock database.
/// </summary>
public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    /// <summary>
    /// Performs simulated credential verification by checking the "X-Username" HTTP header.
    /// </summary>
    /// <returns>The authentication result (Success, Fail, or NoResult).</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check whether the request contains the X-Username header
        if (!Request.Headers.TryGetValue("X-Username", out var usernameHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = usernameHeader.ToString();
        // Retrieve the role associated with that username from the mock user database
        var role = MockUserDatabase.GetRole(username);

        if (string.IsNullOrEmpty(role))
        {
            return Task.FromResult(AuthenticateResult.Fail("User not recognized."));
        }

        // Generate identity claims with username and role for ASP.NET role-based authorization
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "MockAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "MockAuthentication");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}