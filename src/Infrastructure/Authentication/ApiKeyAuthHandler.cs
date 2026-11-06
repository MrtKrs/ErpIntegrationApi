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

public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    //private readonly IConfiguration _configuration;
    private readonly IApiKeyValidationService _apiKeyValidationService;

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyValidationService apiKeyValidationService) : base ( options, logger, encoder)
    {
        _apiKeyValidationService = apiKeyValidationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        if(!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            //return Task.FromResult(AuthenticateResult.NoResult()); 
            return AuthenticateResult.NoResult();
        }

        string authHeaderValue = authorizationHeader.ToString();

        if (!authHeaderValue.StartsWith("Api-Key ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail($"Invalid authorization scheme. Expected 'Api-Key <key>'.");
            //return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme. Expected 'Api-Key <key>'."));
        }

        string extractedKey = authHeaderValue.Substring("Api-Key ".Length).Trim();
        var (isValid, permissions) = await _apiKeyValidationService.ValidateKeysAsync(extractedKey, Context.RequestAborted);
        if(!isValid)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "ApiUser"),
        };
        foreach(var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var identity = new ClaimsIdentity(claims, ApiKeyAuthOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ApiKeyAuthOptions.SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
