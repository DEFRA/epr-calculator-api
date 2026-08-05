using System.Globalization;
using EPR.Calculator.API.Data.Utils;

namespace EPR.Calculator.Service.Function.Utils;

public static class FormatUtils
{
    private static readonly CultureInfo GroupedCurrencyCulture = CreateCulture(",");
    private static readonly CultureInfo UngroupedCurrencyCulture = CreateCulture("");

    private static CultureInfo CreateCulture(string seperator)
    {
        var culture = CultureInfo.CreateSpecificCulture("en-GB");
        culture.NumberFormat.CurrencySymbol = "£";
        culture.NumberFormat.CurrencyGroupSeparator = seperator;
        return culture;
    }

    public static string FormatCurrency(decimal value, int precision = 2, bool useGrouping = false) =>
        MathUtils
            .RoundAwayFromZero(value, precision)
            .ToString($"C{precision}", useGrouping ? GroupedCurrencyCulture : UngroupedCurrencyCulture);

    public static string FormatPercentage(decimal value, int precision = 8) =>
        MathUtils
            .RoundAwayFromZero(value, precision)
             .ToString($"F{precision}") + "%";
}
