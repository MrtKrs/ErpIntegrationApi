using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using IntegrationTests.Common.Database;
using IntegrationTests.Common.Fakers;

namespace IntegrationTests.Products;

public class UpdateProductByIdTests : BaseIntegrationTest
{
    private readonly ProductDbHelper _productDbHelper;
    //private readonly InventoryDbHelper _inventoryDbHelper;

    public UpdateProductByIdTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _productDbHelper = new ProductDbHelper(ConnectionFactory);
        // _inventoryDbHelper = new InventoryDbHelper(ConnectionFactory);
    }
    
    [Fact]
    public async Task ShouldReturnNoContent_AndUpdateDatabase_WhenValid()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();

        var updateRequest = ProductFakers.UpdateRequest.Generate();
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{seededProduct.Id}", updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var dbProduct = await _productDbHelper.GetProductByIdAsync(seededProduct.Id);
        dbProduct.ShouldNotBeNull();
        dbProduct.Sku.ShouldBe(updateRequest.Sku);
        dbProduct.Name.ShouldBe(updateRequest.Name);
        dbProduct.Price.ShouldBe(updateRequest.Price);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var maxId = await _productDbHelper.GetMaxId();
        var updateRequest = ProductFakers.UpdateRequest.Generate();
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{maxId+10000000}", updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();
        var updateRequest = ProductFakers.UpdateRequest.Generate() with { Sku = string.Empty };
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{seededProduct.Id}", updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var dbProduct = await _productDbHelper.GetProductByIdAsync(seededProduct.Id);
        dbProduct.ShouldNotBeNull();
        dbProduct.Sku.ShouldBe(seededProduct.Sku);
        dbProduct.Name.ShouldBe(seededProduct.Name);
        dbProduct.Price.ShouldBe(seededProduct.Price);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenNoApiKeyProvided()
    {
        var updateRequest = ProductFakers.UpdateRequest.Generate();
        
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{1}", updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnForbidden_WhenApiKeyLacksWritePermission()
    {
        var updateRequest = ProductFakers.UpdateRequest.Generate();

        AuthenticateAsReadOnly();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{1}", updateRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
