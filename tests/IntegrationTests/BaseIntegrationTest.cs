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
    internal static string TestErpDatabaseName = "ERP_Test_CustomerA";

    protected IDbConnectionFactory ConnectionFactory { get; }
    protected HttpClient HttpClient { get; }
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        
        using IServiceScope scope = factory.Services.CreateScope();

        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetDatabase(TestErpDatabaseName);
        
        ConnectionFactory = (IDbConnectionFactory)scope.ServiceProvider.GetService(typeof(IDbConnectionFactory))!;
    }

    protected void AuthenticateAsAdmin() => AuthenticateWithKey(AdminApiKey);

    protected void AuthenticateAsReadOnly() => AuthenticateWithKey(ReadOnlyApiKey);

    protected void AuthenticateWithKey(string rawKey)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Api-Key", rawKey);
    }

    protected void ClearAuthentication()
    {
        HttpClient.DefaultRequestHeaders.Authorization = null;
    }
}
