namespace BlueHarbor.Security;

using BlueHarbor.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Username", out var usernameHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = usernameHeader.ToString();
        var role = MockUserDatabase.GetRole(username);

        if (string.IsNullOrEmpty(role))
        {
            return Task.FromResult(AuthenticateResult.Fail("Utente non riconosciuto."));
        }

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