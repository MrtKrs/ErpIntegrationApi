using System.Globalization;
using Bogus;
using Bogus.DataSets;
using IntegrationTests.Common.Database;
using Web.Api.Endpoints.Products;

namespace IntegrationTests.Common.Fakers;

public static class ProductFakers
{
    private const string CultureName = "da-DK";
    private const string PriceFormat = "N2";
    private const string CurrencySuffix = "kr.";
    private const string SeedSkuPrefix = "SKU-";
    private const string UpdateSkuPrefix = "UPD-";
    private const int SkuRandomLength = 8;
    private const decimal MinPrice = 10m;
    private const decimal MaxPrice = 999m;

    private static readonly CultureInfo DanishCulture = CultureInfo.GetCultureInfo(CultureName);

    internal static Faker<Update.Request> UpdateRequest {get;} = new Faker<Update.Request>()
        .CustomInstantiator(f => new Update.Request(
                Sku: GenerateSku(f, SeedSkuPrefix),
                Name: f.Commerce.ProductName(),
                Price: GeneratePrice(f)
        ));

    public static Faker<TestProduct> SeedProduct {get;} = new Faker<TestProduct>()
        .CustomInstantiator(f => new TestProduct(
            Id: 0,
            Sku: GenerateSku(f, UpdateSkuPrefix),
            Name: f.Commerce.ProductName(),
            Price: GeneratePrice(f)
        ));

    private static string GenerateSku(Faker f, string prefix) =>
        $"{prefix}{f.Random.AlphaNumeric(SkuRandomLength).ToUpperInvariant()}";
        
    private static string GeneratePrice(Faker f) =>
        $"{f.Random.Decimal(MinPrice, MaxPrice).ToString(PriceFormat, DanishCulture)} {CurrencySuffix}";
}
