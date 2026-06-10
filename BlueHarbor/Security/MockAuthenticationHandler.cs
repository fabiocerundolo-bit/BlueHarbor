namespace BlueHarbor.Security;

using BlueHarbor.Application.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

/// <summary>
/// Gestore di autenticazione mock per simulare l'accesso degli utenti.
/// Legge l'header personalizzato "X-Username" per identificare l'utente e il suo ruolo dal database mock.
/// </summary>
public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    /// <summary>
    /// Esegue il controllo delle credenziali simulato verificando l'header HTTP "X-Username".
    /// </summary>
    /// <returns>Il risultato dell'autenticazione (Success, Fail o NoResult).</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Verifica se la richiesta contiene l'header X-Username
        if (!Request.Headers.TryGetValue("X-Username", out var usernameHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = usernameHeader.ToString();
        // Recupera il ruolo associato a quell'username dal database utenti mock
        var role = MockUserDatabase.GetRole(username);

        if (string.IsNullOrEmpty(role))
        {
            return Task.FromResult(AuthenticateResult.Fail("Utente non riconosciuto."));
        }

        // Genera i claim di identità con nome utente e ruolo per l'autorizzazione basata su ruoli di ASP.NET
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