using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Pagination;
using Application.Products.Get;
using Infrastructure.Database;
using Dapper;
using Domain.Products;
using System.Diagnostics.Contracts;

namespace IntegrationTests.Products;

public class GetProductsTests : BaseIntegrationTest, IClassFixture<IntegrationTestCollection>
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public GetProductsTests (IntegrationTestWebAppFactory factory) : base(factory)
    {
        _connectionFactory = (IDbConnectionFactory)factory.Services.GetService(typeof(IDbConnectionFactory))!;
    }

    [Fact]
     public async Task GetProducts_ShouldReturnEmptyList()
    {
        using var connection = _connectionFactory.CreateConnection();
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync("products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //PagedList<ProductResponse>
        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
     public async Task GetProducts_ShouldReturnListOfOne()
    {
        using var connection = _connectionFactory.CreateConnection();

        var testProduct = new { Id = 102, Sku = "PROD-102", Name = "Test Wrench", Price = "199.95 kr" };
        await connection.ExecuteAsync(
            "INSERT INTO tblVarer (id, varenummer, varenavn, standardpris) VALUES (@Id, @Sku, @Name, @Price)",
            testProduct);

        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync("products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //PagedList<ProductResponse>
        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnFilteredResults_WhenSearchTermMatches()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO tblVarer (id, varenummer, varenavn, standardpris) VALUES 
            ('71', 'Industrial Widget A', 'WID-A', '99.99'),
            ('72', 'Deluxe Sprocket B', 'SPR-B', '49.50'),
            ('73', 'Gear Ind. C', 'GearIndust-C', '150.00');");

        string searchTerm = "Indust";
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync(
            $"products?page=1&pageSize=10&search={searchTerm}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedList<Product>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item=>
        {
            bool containsInName = item.Name != null && item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            bool containsInSku = item.Sku != null && item.Sku.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

            Assert.True(containsInName || containsInSku);
        });
    }

    [Fact]
    public async Task GetProducts_ShouldEnforcePagination_WhenPageSizeIsSpecified()
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO tblVarer (id, varenummer, varenavn, standardpris) VALUES 
            ('171', 'Prod 171', 'Sku171', '171'),
            ('172', 'Prod 172', 'Sku172', '172'),
            ('173', 'Prod 173', 'Sku173', '173');");

        int pageSize = 2;
        Authenticate("dev-secret-placeholder-key-123");
        HttpResponseMessage response = await HttpClient.GetAsync($"products?page=1&pageSize={pageSize}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.Items.Count);
        Assert.True(result.HasNextPage);
    }
}