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

        var testProduct = new { Id = 101, Sku = "PROD-101", Name = "Test Screwdriver", Price = "99.95 kr" };
        await connection.ExecuteAsync(
            "INSERT INTO tblVarer (id, varenummer, varenavn, standardpris) VALUES (@Id, @Sku, @Name, @Price)",
            testProduct);

        //Act
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products/{testProduct.Id}");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(testProduct.Name, result.Name);
        Assert.Equal(testProduct.Sku, result.Sku);
        Assert.Equal(testProduct.Price, result.Price);
    }

    [Fact]
    public async Task GetProductById_WhenProductNotExists_ShouldReturnNotFound()
    {
        using var connection = _connectionFactory.CreateConnection();
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products/70");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductNotExists_ShouldReturnNotFoundTwo()
    {
        using var connection = _connectionFactory.CreateConnection();
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products/72");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductNotExists_ShouldReturnNotFoundThree()
    {
        using var connection = _connectionFactory.CreateConnection();
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products/73");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductNotExists_ShouldReturnNotFoundFour()
    {
        using var connection = _connectionFactory.CreateConnection();
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products/171");

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
