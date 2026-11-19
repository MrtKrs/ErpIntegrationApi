using Application.Abstractions.Data;
using Dapper;

namespace IntegrationTests.Common.Database;

public sealed record TestInventory(
    int ProductId,
    bool? UseInventory,
    int? Stock,
    int? LowLimit,
    int? Reserved,
    int? Available,
    int? Ordered);

public sealed class InventoryDbHelper(IDbConnectionFactory connectionFactory)
{
    public async Task<TestInventory> SeedInventoryAsync(
        int productId, 
        bool? useInventory = true,
        int? stock = 10,
        int? lowLimit = 2,
        int? reserved = 0,
        int? available = 10,
        int? ordered = 0)
    {
        var inventory = new TestInventory(
            productId,
            useInventory,
            stock,
            lowLimit,
            reserved,
            available,
            ordered);

        using var connection = connectionFactory.CreateErpConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO [dbo].[tblLager]
                ([vareid],[benytlagerstyring],[beholdning],[lavgrænse],[reserveret],[disponibel],[bestilt])
            VALUES
                (@ProductId, @UseInventory, @Stock, @LowLimit, @Reserved, @Available, @Ordered);", 
            inventory);
        
        return inventory;
    }
}