using Application.Abstractions.Data;
using Dapper;

namespace IntegrationTests.Common.Seeders;

public sealed record SeededInventory(
    int ProductId,
    bool? UseInventory,
    int? Stock,
    int? LowLimit,
    int? Reserved,
    int? Available,
    int? Ordered);

public sealed class InventorySeeder(IDbConnectionFactory connectionFactory)
{
    public async Task<SeededInventory> SeedInventoryAsync(
        int productId, 
        bool? useInventory = true,
        int? stock = 10,
        int? lowLimit = 2,
        int? reserved = 0,
        int? available = 10,
        int? ordered = 0)
    {
        var inventory = new SeededInventory(
            productId,
            useInventory,
            stock,
            lowLimit,
            reserved,
            available,
            ordered);

        using var connection = connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO [dbo].[tblLager]
                ([vareid],[benytlagerstyring],[beholdning],[lavgrænse],[reserveret],[disponibel],[bestilt])
            VALUES
                (@ProductId, @UseInventory, @Stock, @LowLimit, @Reserved, @Available, @Ordered);", 
            inventory);
        
        return inventory;
    }
}