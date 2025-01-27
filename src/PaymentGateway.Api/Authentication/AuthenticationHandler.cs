using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PaymentGateway.Api.Authentication;

public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string HeaderKey = "Authorization";

    public AuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderKey, out var merchantIdentifier))
            return Task.FromResult(AuthenticateResult.Fail("Authorization Header not provided"));

        var claims = new[] {
            new Claim(ClaimTypes.Name, merchantIdentifier)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
