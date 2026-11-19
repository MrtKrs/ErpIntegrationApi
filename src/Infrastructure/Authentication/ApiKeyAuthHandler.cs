using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKeyScheme";
}

public class ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidationService apiKeyValidationService,
        ITenantContext tenantContext) : AuthenticationHandler<ApiKeyAuthOptions>( options, logger, encoder)
{


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        if(!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        string authHeaderValue = authorizationHeader.ToString();

        if (!authHeaderValue.StartsWith("Api-Key ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail($"Invalid authorization scheme. Expected 'Api-Key <key>'.");
        }

        string extractedKey = authHeaderValue.Substring("Api-Key ".Length).Trim();
        var validationResult = await apiKeyValidationService.ValidateAsync(extractedKey, Context.RequestAborted);
        if(validationResult is null)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        tenantContext.SetDatabase(validationResult.DatabaseName);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, validationResult.ClientId),
            new Claim(ClaimTypes.Name, validationResult.ClientName),
        };
        
        claims.AddRange(validationResult.Permissions.Select(p=> new Claim("permissions", p)));

        var identity = new ClaimsIdentity(claims, ApiKeyAuthOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthOptions.SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
