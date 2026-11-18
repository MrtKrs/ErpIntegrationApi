using System;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Application.Abstractions.Data;
using Application.Products.GetById;
using Dapper;
using Domain.Products;
using IntegrationTests.Common.Database;

namespace IntegrationTests.Products;

public sealed class GetProductByIdTests : BaseIntegrationTest, IClassFixture<IntegrationTestCollection>
{
    private readonly ProductDbHelper _productDbHelper;
    private readonly InventoryDbHelper _inventoryDbHelper;

    public GetProductByIdTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _productDbHelper = new ProductDbHelper(ConnectionFactory);
        _inventoryDbHelper = new InventoryDbHelper(ConnectionFactory);
    }

    [Fact]
    public async Task WhenProductExists_ShouldReturnProductData()
    {
        //Arrange
        var seededProduct = await _productDbHelper.SeedProductAsync(
            sku: "PROD-101",
            name : "Test Screwdriver",
            price : "99.95 kr");

        //Act
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync($"products/{seededProduct.Id}");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(seededProduct.Id, result.Id);
        Assert.Equal(seededProduct.Sku, result.Sku);
        Assert.Equal(seededProduct.Name, result.Name);
        Assert.Equal(seededProduct.Price, result.Price);
    }

    [Fact]
    public async Task WhenProductExists_ShouldReturnProductWithInventory()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();

        var seededInventory = await _inventoryDbHelper.SeedInventoryAsync(productId: seededProduct.Id, stock: 67, available: 57);

        AuthenticateAsAdmin();
        var response = await HttpClient.GetAsync($"products/{seededProduct.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(seededProduct.Id, product.Id);
        Assert.Equal(seededInventory.Stock, product.StockQuantity);
        Assert.Equal(seededInventory.Available, product.AvailableQuantity); 
    }

    [Fact]
    public async Task WhenProductNotExists_ShouldReturnNotFound()
    {
        var maxId = await _productDbHelper.GetMaxId();
        AuthenticateAsAdmin();

        HttpResponseMessage response = await HttpClient.GetAsync($"products/{maxId + 1000000}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    

    [Fact]
    public async Task WhenNoApiKeyIsProvided_ShouldReturnUnauthorized()
    {
        //Arrange
        int productId = 678;

        //Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/products/{productId}");

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
