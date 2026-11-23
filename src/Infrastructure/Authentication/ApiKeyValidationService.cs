using System;
using Dapper;
using Application.Abstractions.Data;
using Application.Abstractions.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Authentication;

internal sealed class ApiKeyValidationService(
    IDbConnectionFactory connectionFactory) : IApiKeyValidationService
{
    private sealed record ApiKeyDbRow(
        int ClientId,
        string ClientName,
        string DatabaseName,
        string PermissionsJson);

    public async Task<ApiKeyValidationResult?> ValidateAsync (string apiKey, CancellationToken cancellationToken)
    {
        // TODO: short IMemoryCache ?
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        string keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
        const string sql = """
            SELECT 
                c.[id] AS ClientId, 
                c.[clientname] AS ClientName,
                c.[databasename] AS DatabaseName,
                c.[permissionsjson] AS PermissionsJson
            FROM dbo.[tblApiKey] k 
            INNER JOIN dbo.[tblApiClient] c ON k.[clientid] = c.[id]
            WHERE k.[keyhash] = @KeyHash
                AND k.[isrevoked] = 0
                AND c.[isactive] = 1
                AND (k.[expiresatutc] IS NULL OR k.[expiresatutc] > SYSUTCDATETIME());
            """;
        
        using var connection = connectionFactory.CreateAuthConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ApiKeyDbRow>(
            new CommandDefinition(sql, new {KeyHash = keyHash}, cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        string[] permissions = JsonSerializer.Deserialize<string[]>(row.PermissionsJson )?? [];

        return new ApiKeyValidationResult(
            ClientId: row.ClientId,
            ClientName: row.ClientName,
            DatabaseName: row.DatabaseName,
            Permissions: permissions);
    }
}
