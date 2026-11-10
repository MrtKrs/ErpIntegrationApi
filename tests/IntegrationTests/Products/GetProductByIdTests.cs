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
        //Arrange
        using var connection = _connectionFactory.CreateConnection();

        var testProduct = new { Sku = "PROD-101", Name = "Test Screwdriver", Price = "99.95 kr" };
        var id = await connection.QuerySingleAsync<int>(@"
        INSERT INTO tblVarer (varenummer, varenavn, standardpris) VALUES (@Sku, @Name, @Price);
                SELECT SCOPE_IDENTITY();", 
            testProduct);

        //Act
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync($"products/{id}");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(testProduct.Name, result.Name);
        Assert.Equal(testProduct.Sku, result.Sku);
        Assert.Equal(testProduct.Price, result.Price);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProductWithInventory()
    {
        using var connection = _connectionFactory.CreateConnection();

        var testProduct = new { Sku = "Prod w Stock", Name = "Product with Stock", Price = "99.95 kr" };
        var productId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO tblVarer (varenummer, varenavn, standardpris) VALUES (@Sku, @Name, @Price);
            SELECT SCOPE_IDENTITY();", 
            testProduct);
        var testStock = new { ProductId = productId, UseInventory = true, @Stock = 67, @LowLimit = 10, @Reserved = 10, @Available = 57, @Ordered = 20};
        await connection.ExecuteAsync(@"
            INSERT INTO [dbo].[tblLager]
                ([vareid],[benytlagerstyring],[beholdning],[lavgrænse],[reserveret],[disponibel],[bestilt])
            VALUES
                (@ProductId, @UseInventory, @Stock, @LowLimit, @Reserved, @Available, @Ordered);", 
           testStock);

        AuthenticateAsAdmin();

        var response = await HttpClient.GetAsync($"products/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(testStock.Stock, product.StockQuantity);
        Assert.Equal(testStock.Available, product.AvailableQuantity); 
    }

    [Fact]
    public async Task GetProductById_WhenProductNotExists_ShouldReturnNotFound()
    {
        using var connection = _connectionFactory.CreateConnection();
        var maxId = await connection.QuerySingleAsync<int>(@"
        SELECT COALESCE(MAX(id), 0) FROM tblVarer
        ");
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync($"products/{maxId+1000000}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenNoApiKeyIsProvided_ShouldReturnUnauthorized()
    {
        //Arrange
        int productId = 678;

        //Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/products/{productId}");

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
