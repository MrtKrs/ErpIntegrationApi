using Application.Abstractions.Data;
using Dapper;

namespace IntegrationTests.Common.Database;

public sealed record TestProduct(
    int Id, 
    string Sku, 
    string Name, 
    string Price);

public sealed class ProductDbHelper(IDbConnectionFactory connectionFactory)
{
    public async Task<TestProduct> SeedProductAsync(
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
        
        return new TestProduct(id, resolvedSku, resolvedName, price);
    }

    public async Task<TestProduct?> GetProductByIdAsync(int id)
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TestProduct>(@"
            SELECT 
                id AS Id,
                varenummer AS Sku,
                varenavn AS Name,
                standardpris AS Price
            FROM tblVarer
            WHERE id = @Id;",
            new { Id = id });
    }

    internal async Task<int> GetMaxId()
    {
        using var connection = connectionFactory.CreateConnection();
        
        var maxId = await connection.QuerySingleAsync<int>(@"
        SELECT COALESCE(MAX(id), 0) FROM tblVarer
        ");
        return maxId;
    }
}