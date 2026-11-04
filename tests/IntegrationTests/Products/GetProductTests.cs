using System.Net;
using Xunit;

namespace IntegrationTests.Products;

public class GetProductTests : BaseIntegrationTest, IClassFixture<IntegrationTestWebAppFactory>
{
    public GetProductTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact] 
    public async Task GetProduct_WhenEndpointDoesNotExist_ShouldReturnNotFoundOrUnimplemented()
    {
        // ACT: Hit an endpoint that we haven't built yet
        HttpResponseMessage response = await HttpClient.GetAsync("api/products/1");

        // It will fail because the API endpoint isn't mapped in Web.Api yet.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
