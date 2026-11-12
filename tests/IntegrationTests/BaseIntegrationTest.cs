using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Abstractions.Data;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest
{
    protected IDbConnectionFactory ConnectionFactory { get; }
    protected HttpClient HttpClient { get; }
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        ConnectionFactory = (IDbConnectionFactory)factory.Services.GetService(typeof(IDbConnectionFactory))!;
    }

    protected void AuthenticateAsAdmin()
    {
        Authenticate("dev-secret-placeholder-key-123");
    }
    
    private void Authenticate(string accessToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", accessToken);
    }
}
