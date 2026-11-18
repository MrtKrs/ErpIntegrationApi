using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Pagination;
using Application.Products.Get;
using Infrastructure.Database;
using Dapper;
using Domain.Products;
using System.Diagnostics.Contracts;
using IntegrationTests.Common.Database;

namespace IntegrationTests.Products;

public class GetProductsTests : BaseIntegrationTest, IClassFixture<IntegrationTestCollection>
{
    private readonly ProductDbHelper _productDbHelper;
    
    public GetProductsTests (IntegrationTestWebAppFactory factory) : base(factory)
    {
        _productDbHelper = new ProductDbHelper(ConnectionFactory);
    }

    [Fact]
     public async Task GetProducts_ShouldReturnEmptyList_WhenNoSearchMatches()
    {
        string searchTerm = $"{Guid.NewGuid():N}";
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync(
            $"products?page=1&pageSize=10&search={searchTerm}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedList<Product>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFilteredResults_WhenSearchMatches()
    {
        string scope = Guid.NewGuid().ToString("N")[..8];

        // Match 1: Matches via Name ('varenavn')
        TestProduct widget = await _productDbHelper.SeedProductAsync(
            sku: $"Sku1-{scope}",
            name: $"Widget Industrial-{scope}",
            price: "99.99");

        // Non-match: Should be excluded by the search filter
        await _productDbHelper.SeedProductAsync(
            sku: $"Sku2-{scope}",
            name: $"Sprocket {scope}",
            price: "49.50");

        // Match 2: Matches via SKU ('varenummer')
        TestProduct gear = await _productDbHelper.SeedProductAsync(
            sku: $"Sku3-Industrial-{scope}",
            name: $"Gear {scope}",
            price: "150.00");
        
        string searchTerm = $"Industrial-{scope}";
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync(
            $"products?page=1&pageSize=10&search={searchTerm}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedList<Product>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, p => p.Id == widget.Id);
        Assert.Contains(result.Items, p => p.Id == gear.Id);
    }

    [Fact]
    public async Task GetProducts_ShouldEnforcePagination_WhenPageSizeIsSpecified()
    {
        string scope = Guid.NewGuid().ToString("N")[..8];

        for(int i = 1; i<=3; i++) 
        {
            await _productDbHelper.SeedProductAsync(sku: $"Sku{i}-{scope}");
        }

        int pageSize = 2;
        AuthenticateAsAdmin();
            HttpResponseMessage response = await HttpClient.GetAsync(
        $"products?page=1&pageSize={pageSize}&search={scope}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.Items.Count);
        Assert.True(result.HasNextPage);
    }
}