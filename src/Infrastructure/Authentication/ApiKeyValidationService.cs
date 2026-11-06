using System;
using Dapper;
using Application.Abstractions.Data;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class ApiKeyValidationService(IDbConnectionFactory connectionFactory) : IApiKeyValidationService
{
    public async Task<(bool IsValild, string[] Permissions)> ValidateKeysAsync(string apiKey, CancellationToken cancellationToken)
    {
        _ = connectionFactory;
        // Example:
        // using var connection = connectionFactory.CreateConnection();
        // const string sql = @"
        //     SELECT p.PermissionName 
        //     FROM ApiKeys k
        //     JOIN ApiKeyPermissions p ON k.Id = p.ApiKeyId
        //     WHERE k.KeyString = @ApiKey AND k.IsActive = 1";
        //
        // var permissions = await connection.QueryAsync<string>(
        //      new CommandDefinition(sql, new { ApiKey = apiKey }, cancellationToken: cancellationToken));
        // return (permissions.Any(), permissions.ToArray());

        //TODO: replace with look up from storage tables as example above
        if (apiKey == "dev-secret-placeholder-key-123")
        {
            var mockPermissions = new[] { "products:read" };
            return (true, mockPermissions);
        }

        return (false, Array.Empty<string>());
    }
}
