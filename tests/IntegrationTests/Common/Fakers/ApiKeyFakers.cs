using Bogus;
using IntegrationTests.Common.Database;

namespace IntegrationTests.Common.Fakers;

public static class ApiKeyFakers
{
    public static Faker<TestApiKey> SeedApiKey {get;} = new Faker<TestApiKey>()
        .CustomInstantiator(f => new TestApiKey(
            RawKey: $"sk_test_{f.Random.AlphaNumeric(24)}",
            ClientId: 0,
            ClientName: f.Company.CompanyName(),
            DatabaseName: $"ERP_Test_{f.Random.AlphaNumeric(8)}",
            Permissions: ["products:read", "products:write"]
        ));
}
