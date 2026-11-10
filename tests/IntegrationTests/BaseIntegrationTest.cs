using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest
{
     protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
    }

    protected HttpClient HttpClient { get; }

    protected void AuthenticateAsAdmin()
    {
        Authenticate("dev-secret-placeholder-key-123");
    }
    
    private void Authenticate(string accessToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", accessToken);
    }
}
