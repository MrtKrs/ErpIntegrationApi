using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest
{
    internal static string AdminApiKey = "test-admin-key";
    internal static string ReadOnlyApiKey = "test-readonly-key";

    protected IDbConnectionFactory ConnectionFactory { get; }
    protected HttpClient HttpClient { get; }
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        
        using IServiceScope scope = factory.Services.CreateScope();

        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetDatabase(IntegrationTestWebAppFactory.TestErpDatabaseName);
        
        ConnectionFactory = (IDbConnectionFactory)scope.ServiceProvider.GetService(typeof(IDbConnectionFactory))!;
    }

    protected void AuthenticateAsAdmin()
    {
        Authenticate(AdminApiKey);
    }

    protected void AuthenticateAsReadOnly()
    {
        Authenticate(ReadOnlyApiKey);
    }
    
    private void Authenticate(string accessToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", accessToken);
    }
}
