using Bogus;
using Web.Api.Endpoints.Products;

namespace IntegrationTests.Common.Fakers;

public static class ProductFakers
{
    internal static Faker<Update.Request> UpdateRequest {get;} = new Faker<Update.Request>("sv")
        .CustomInstantiator(f => new Update.Request(
                Sku: $"UPD-{f.Random.AlphaNumeric(8).ToUpperInvariant()}",
                Name: f.Commerce.ProductName(),
                Price: $"{f.Random.Decimal(10, 999):N2} kr."
        ));
}
