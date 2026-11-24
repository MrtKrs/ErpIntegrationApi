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

public sealed class GetProductByIdTests : BaseIntegrationTest
{
    private readonly ProductDbHelper _productDbHelper;
    private readonly InventoryDbHelper _inventoryDbHelper;

    public GetProductByIdTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _productDbHelper = new ProductDbHelper(ConnectionFactory);
        _inventoryDbHelper = new InventoryDbHelper(ConnectionFactory);
    }

    [Fact]
    public async Task ShouldReturnProductData_WhenProductExists()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();

        AuthenticateAsReadOnly();
        HttpResponseMessage response = await HttpClient.GetAsync($"products/{seededProduct.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(seededProduct.Id);
        result.Sku.ShouldBe(seededProduct.Sku);
        result.Name.ShouldBe(seededProduct.Name);
        result.Price.ShouldBe(seededProduct.Price);
    }

    [Fact]
    public async Task ShouldReturnProductWithInventory_WhenProductExists()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();

        var seededInventory = await _inventoryDbHelper.SeedInventoryAsync(productId: seededProduct.Id, stock: 67, available: 57);

        AuthenticateAsAdmin();
        var response = await HttpClient.GetAsync($"products/{seededProduct.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ProductResponse? result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(seededProduct.Id);
        ((int?)result.StockQuantity).ShouldBe(seededInventory.Stock);
        ((int?)result.AvailableQuantity).ShouldBe(seededInventory.Available);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenProductNotExists()
    {
        var maxId = await _productDbHelper.GetMaxId();
        AuthenticateAsAdmin();

        HttpResponseMessage response = await HttpClient.GetAsync($"products/{maxId + 1000000}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenNoApiKeyIsProvided()
    {
        int productId = 678;

        HttpResponseMessage response = await HttpClient.GetAsync($"/products/{productId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
