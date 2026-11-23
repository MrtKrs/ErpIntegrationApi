using System.Net;
using IntegrationTests.Common.Database;

namespace IntegrationTests.Authentication;

public class ApiKeyValidationServiceTests : BaseIntegrationTest
{
    private readonly ApiKeyDbHelper _apiKeyDbHelper;
    public ApiKeyValidationServiceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _apiKeyDbHelper = new ApiKeyDbHelper(ConnectionFactory);
    }

    [Fact]
    public async Task ShouldReturnOk_WhenApiKeyIsValidAndHasPermission()
    {
        TestApiKey testKey = await _apiKeyDbHelper.SeedApiKeyAsync( k => k with
        {
           DatabaseName = TestErpDatabaseName,
           Permissions = ["products:read"] 
        });

        AuthenticateWithKey(testKey.RawKey);
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        ClearAuthentication();
        HttpResponseMessage response = await HttpClient.GetAsync("products");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenApiKeyDoesNotExist()
    {
        AuthenticateWithKey($"sk_test_nonexistant_{Guid.NewGuid():N}");
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task ShouldReturnUnauthorized_WhenApiKeyIsRevoked()
    {
        TestApiKey testKey = await _apiKeyDbHelper.SeedApiKeyAsync( k => k with
        {
            DatabaseName = TestErpDatabaseName,
            Permissions = ["products:read"],
            IsRevoked = true
        });

        AuthenticateWithKey(testKey.RawKey);
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenApiKeyIsExpired()
    {
        TestApiKey testKey = await _apiKeyDbHelper.SeedApiKeyAsync( k => k with
        {
            DatabaseName = TestErpDatabaseName,
            Permissions = ["products:read"],
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1)
        });

        AuthenticateWithKey(testKey.RawKey);
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnUnauthorized_WhenClientIsInactive()
    {
        TestApiKey testKey = await _apiKeyDbHelper.SeedApiKeyAsync( k => k with
        {
            DatabaseName = TestErpDatabaseName,
            Permissions = ["products:read"],
            IsActive = false
        });

        AuthenticateWithKey(testKey.RawKey);
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldReturnForbidden_WhenApiKeyLacksPermission()
    {
        TestApiKey testKey = await _apiKeyDbHelper.SeedApiKeyAsync( k => k with
        {
            DatabaseName = TestErpDatabaseName,
            Permissions = ["orders:read"],
        });

        AuthenticateWithKey(testKey.RawKey);
        HttpResponseMessage response = await HttpClient.GetAsync("products?page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
