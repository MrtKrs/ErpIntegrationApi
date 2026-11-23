using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Data;
using Dapper;
using IntegrationTests.Common.Fakers;

namespace IntegrationTests.Common.Database;

public sealed record TestApiKey(
    string RawKey,
    int ClientId,
    string ClientName,
    string DatabaseName,
    string[] Permissions,
    bool IsActive = true,
    bool IsRevoked = false,
    DateTime? ExpiresAtUtc = null);
public sealed class ApiKeyDbHelper(IDbConnectionFactory connectionFactory)
{
    public async Task<TestApiKey> SeedApiKeyAsync(Func<TestApiKey, TestApiKey>? customize = null)
    {
        var fake = ApiKeyFakers.SeedApiKey.Generate();
        var apiKey = customize is null ? fake : customize(fake);

        string keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey.RawKey)));
        string permissionsJson = JsonSerializer.Serialize(apiKey.Permissions);

        const string sqlClient = """
            INSERT INTO [tblApiClient] ([clientname], [databasename], [permissionsjson], [isactive])
            OUTPUT INSERTED.id
            VALUES (@ClientName, @DatabaseName, @PermissionsJson, @IsActive);
            """;
        const string sqlKey = """
            INSERT INTO [tblApiKey] ([keyhash], [clientid], [expiresatutc], [isrevoked])
            VALUES (@KeyHash, @ClientId, @ExpiresAtUtc, @IsRevoked);
            """;

        using var connection = connectionFactory.CreateAuthConnection();
        int clientId = await connection.QuerySingleAsync<int>(sqlClient, new
        {
            apiKey.ClientName,
            apiKey.DatabaseName,
            PermissionsJson = permissionsJson,
            apiKey.IsActive,
        });

        await connection.ExecuteAsync(sqlKey, new 
        {
            KeyHash = keyHash,
            ClientId = clientId,
            apiKey.ExpiresAtUtc,
            apiKey.IsRevoked,
        });
        return apiKey with{ ClientId = clientId };
    }
}
