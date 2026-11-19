using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using IntegrationTests.Common.Database;
using IntegrationTests.Common.Fakers;

namespace IntegrationTests.Products;

public class UpdateProductByIdTests : BaseIntegrationTest
{
    // TODO:
    // ShouldNotModifyStock_WhenUpdatingProductDetails

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

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var dbProduct = await _productDbHelper.GetProductByIdAsync(seededProduct.Id);
        Assert.NotNull(dbProduct);
        Assert.Equal(updateRequest.Sku, dbProduct.Sku);
        Assert.Equal(updateRequest.Name, dbProduct.Name);
        Assert.Equal(updateRequest.Price, dbProduct.Price);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var maxId = await _productDbHelper.GetMaxId();
        var updateRequest = ProductFakers.UpdateRequest.Generate();
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{maxId+10000000}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        var seededProduct = await _productDbHelper.SeedProductAsync();
        var updateRequest = ProductFakers.UpdateRequest.Generate() with { Sku = string.Empty };
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{seededProduct.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var dbProduct = await _productDbHelper.GetProductByIdAsync(seededProduct.Id);
        Assert.NotNull(dbProduct);
        Assert.Equal(seededProduct.Sku, dbProduct.Sku);
        Assert.Equal(seededProduct.Name, dbProduct.Name);
        Assert.Equal(seededProduct.Price, dbProduct.Price);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenNoApiKeyProvided()
    {
        var updateRequest = ProductFakers.UpdateRequest.Generate();
        
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{1}", updateRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnForbidden_WhenApiKeyLacksWritePermission()
    {
        var updateRequest = ProductFakers.UpdateRequest.Generate();

        AuthenticateAsReadOnly();
        HttpResponseMessage response = await HttpClient.PutAsJsonAsync(
            $"products/{1}", updateRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
