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

public class GetProductsTests : BaseIntegrationTest
{
    private readonly ProductDbHelper _productDbHelper;
    
    public GetProductsTests (IntegrationTestWebAppFactory factory) : base(factory)
    {
        _productDbHelper = new ProductDbHelper(ConnectionFactory);
    }

    [Fact]
     public async Task ShouldReturnEmptyList_WhenNoSearchMatches()
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
    public async Task ShouldReturnFilteredResults_WhenSearchMatches()
    {
        string searchTerm = $"Industrial-{Guid.NewGuid().ToString("N")[..8]}";

        // Non-match: Default Bogus data won't contain searchTerm
        await _productDbHelper.SeedProductAsync();

        // Match 1: Matches via Name ('varenavn')
        TestProduct widget = await _productDbHelper.SeedProductAsync( p => 
            p with{Name = $"Widget {searchTerm}"});

        // Match 2: Matches via SKU ('varenummer')
        TestProduct gear = await _productDbHelper.SeedProductAsync( p => 
            p with {Sku = $"Sku-{searchTerm}"});
        
        AuthenticateAsAdmin();
        HttpResponseMessage response = await HttpClient.GetAsync(
            $"products?page=1&pageSize=10&search={Uri.EscapeDataString(searchTerm)}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, p => p.Id == widget.Id);
        Assert.Contains(result.Items, p => p.Id == gear.Id);
    }

    [Fact]
    public async Task ShouldEnforcePagination_WhenPageSizeIsSpecified()
    {
        string scope = Guid.NewGuid().ToString("N")[..8];
        const int seededCount = 3;
        const int pageSize = 2;

        for(int i = 1; i<=seededCount; i++) 
        {
            await _productDbHelper.SeedProductAsync( p => 
                p with {Sku = $"Sku{i}-{scope}"});
        }

        AuthenticateAsAdmin();
            HttpResponseMessage response = await HttpClient.GetAsync(
        $"products?page=1&pageSize={pageSize}&search={scope}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.Items.Count);
        Assert.Equal(seededCount, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}