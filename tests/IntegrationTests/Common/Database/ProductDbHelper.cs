using Application.Abstractions.Data;
using Dapper;
using IntegrationTests.Common.Fakers;

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
        string? price = null)
    {
        var fake = ProductFakers.SeedProduct.Generate();

        var product = fake with
        {
            Sku = sku ?? fake.Sku,
            Name = name ?? fake.Name,
            Price = price ?? fake.Price
        };

        using var connection = connectionFactory.CreateConnection();
        int id = await connection.QuerySingleAsync<int>(@"
            INSERT INTO tblVarer (varenummer, varenavn, standardpris) 
            OUTPUT INSERTED.id
            VALUES (@Sku, @Name, @Price);",
            product);
        
        return product with{ Id = id };
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