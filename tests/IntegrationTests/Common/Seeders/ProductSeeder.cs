using Application.Abstractions.Data;
using Dapper;

namespace IntegrationTests.Common.Seeders;

public sealed record SeededProduct(
    int Id, 
    string Sku, 
    string Name, 
    string Price);

public sealed class ProductSeeder(IDbConnectionFactory connectionFactory)
{
    public async Task<SeededProduct> SeedProductAsync(
        string? sku = null,
        string? name = null,
        string price = "0.00 kr.")
    {
        string uniqueToken = Guid.NewGuid().ToString("N")[..8];

        string resolvedSku = sku ?? $"SKU-{uniqueToken}";
        string resolvedName = name ?? $"Product {uniqueToken}";

        using var connection = connectionFactory.CreateConnection();
        int id = await connection.QuerySingleAsync<int>(@"
            INSERT INTO tblVarer (varenummer, varenavn, standardpris) 
            OUTPUT INSERTED.id
            VALUES (@Sku, @Name, @Price)
            SELECT SCOPE_IDENTITY();",
            new {Sku = resolvedSku, Name = resolvedName, Price = price});
        
        return new SeededProduct(id, resolvedSku, resolvedName, price);
    }
}