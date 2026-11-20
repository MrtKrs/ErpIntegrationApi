using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Pagination;
using Application.Orders.Get;
using IntegrationTests.Common.Database;
using SharedKernel;

namespace IntegrationTests.Orders;

public class GetOrdersTests : BaseIntegrationTest
{
    private readonly OrderDbHelper _orderDbHelper;
    
    public GetOrdersTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _orderDbHelper = new OrderDbHelper(ConnectionFactory);
    }

    [Fact]
    public async Task ShouldReturnFilteredResults_WhenSearchMatches()
    {
        string searchTerm = $"Acme-{Guid.NewGuid().ToString("N")[..8]}";

        await _orderDbHelper.SeedOrderAsync();

        TestOrder customerMatch = await _orderDbHelper.SeedOrderAsync( o => o with
        {
           CustomerName = $"{searchTerm} ApS"
        });

        TestOrder documentMatch = await _orderDbHelper.SeedOrderAsync( o => o with
        {
           DocumentId = $"ORD-{searchTerm}"
        });

        AuthenticateAsReadOnly();
        HttpResponseMessage response = await HttpClient.GetAsync(
            $"orders?page=1&pageSize=10&search={Uri.EscapeDataString(searchTerm)}"
        );

        string responseBody = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, responseBody);

        var result = await response.Content.ReadFromJsonAsync<PagedList<OrderResponse>>();
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Items.ShouldContain(o => o.Id == customerMatch.Id );
        result.Items.ShouldContain(o => o.Id == documentMatch.Id );
    }
}
