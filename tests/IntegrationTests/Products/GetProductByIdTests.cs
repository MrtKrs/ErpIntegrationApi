using System;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Application.Abstractions.Data;
using Application.Products.GetById;
using Dapper;
using Domain.Products;
using Xunit;

namespace IntegrationTests.Products;

public class GetProductByIdTests : BaseIntegrationTest, IClassFixture<IntegrationTestCollection>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetProductByIdTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _connectionFactory = (IDbConnectionFactory)factory.Services.GetService(typeof(IDbConnectionFactory))!;
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ShouldReturnProductData()
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='tblVarer' AND xtype='U')
                CREATE TABLE [dbo].[tblVarer](
                [id] [int] NOT NULL,
                [varenummer] [nvarchar](50) NULL,
                [varenavn] [nvarchar](1000) NULL,
                [standardpris] [nvarchar](50) NULL
           );");

           var testProduct = new { Id = 101, Sku = "PROD-101", Name = "Test Screwdriver", Price = "99.95 kr" };
        await connection.ExecuteAsync(
            "INSERT INTO tblVarer (id, varenummer, varenavn, standardpris) VALUES (@Id, @Sku, @Name, @Price)",
            testProduct);

        HttpResponseMessage response = await HttpClient.GetAsync("products/101");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(testProduct.Name, result.Name);
        Assert.Equal(testProduct.Sku, result.Sku);
        Assert.Equal(testProduct.Price, result.Price);
    }    
}
