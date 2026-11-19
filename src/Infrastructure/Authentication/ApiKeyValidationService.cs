using System;
using Dapper;
using Application.Abstractions.Data;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class ApiKeyValidationService(
    IDbConnectionFactory connectionFactory,
    IReadOnlyDictionary<string, ApiKeyValidationResult>? keys = null) : IApiKeyValidationService
{
    private static readonly Dictionary<string, ApiKeyValidationResult> DefaultDevKeys = new(StringComparer.Ordinal)
    {
        // DatabaseNames need to exist when running locally, this could be extracted to config 
        // or replaced completely when able to auth against DB.
        ["dev-admin-key-A"] = new(
            ClientId: "1",
            ClientName: "Dev Admin A",
            DatabaseName: "ERP_Dev_CustomerA",
            Permissions: ["products:read", "products:write", "orders:read", "orders:write"]),
        ["dev-readonly-key-A"] = new(
            ClientId: "2",
            ClientName: "Dev Reader A ",
            DatabaseName: "ERP_Dev_CustomerA",
            Permissions: ["products:read", "orders:read"]),
        ["dev-admin-key-B"] = new(
            ClientId: "91",
            ClientName: "Dev Admin B",
            DatabaseName: "ERP_Dev_CustomerB",
            Permissions: ["products:read", "products:write", "orders:read", "orders:write"]),
        ["dev-readonly-key-B"] = new(
            ClientId: "92",
            ClientName: "Dev Reader B",
            DatabaseName: "ERP_Dev_CustomerB",
            Permissions: ["products:read", "orders:read"]),
    };

    private readonly IReadOnlyDictionary<string, ApiKeyValidationResult> _keys = keys ?? DefaultDevKeys;

    public async Task<ApiKeyValidationResult?> ValidateAsync (string apiKey, CancellationToken cancellationToken)
    {
        // TODO IMemoryCache?
        
        // Remove when implementing:
        _ = connectionFactory;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }
        _keys.TryGetValue(apiKey, out var result);
        return await Task.FromResult(result);
        
        // HASH apiKey and Validate against hashed key in DB
        // Example:
        // using var connection = connectionFactory.CreateAuthConnection();
        // const string sql = @"
        //  SELECT 
        //      k.Id AS ClientId,
        //      k.ClientName,
        //      k.DatabaseName,
        //      p.PermissionName
        //  FROM ApiKeys k
        //  LEFT JOIN ApiKeyPermissions p ON p.ApiKeyId = k.Id
        //      WHERE k.KeyHash = @KeyHash
        //      AND k.IsActive = 1
        //      AND (k.ExpiresAtUtc IS NULL OR k.ExpiresAtUtc > GETUTCDATE());"

        //return null;
    }
}
