using System.Globalization;

namespace IntegrationTests.Common.Fakers;

internal static class FakerDefaults
{
    private const string CultureName = "da-DK";
    private const string AmountFormat = "N2";
    private const string CurrencySuffix = "kr.";

    public static readonly CultureInfo DanishCulture = CultureInfo.GetCultureInfo(CultureName);

    public static string FormatAmount(decimal amount) =>
        $"{amount.ToString(AmountFormat, DanishCulture)} {CurrencySuffix}";
}
