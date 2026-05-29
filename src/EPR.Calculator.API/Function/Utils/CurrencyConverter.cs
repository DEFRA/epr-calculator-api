using System.Globalization;

namespace EPR.Calculator.Service.Function.Utils
{
    public static class CurrencyConverterUtils
    {
        /// <summary>
        /// Converts a decimal input to GBP currency format (e.g., £12.3456) if valid.
        /// Returns an empty string if input is not valid.
        /// </summary>
        public static string ConvertToCurrency(decimal detail, int precision = 2)
        {
            return FormatCurrencyWithGbpSymbol(detail, precision);
        }

        /// <summary>
        /// Formats decimal to gbp currency.
        /// </summary>
        /// <returns>gbp currency.</returns>
        public static string FormatCurrencyWithGbpSymbol(decimal decimalValue, int precision, string currencyGroupSeparator = "")
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            culture.NumberFormat.CurrencyGroupSeparator = currencyGroupSeparator;
            return decimalValue.ToString($"C{precision}", culture);
        }

        /// <summary>
        /// Converts a decimal input to GBP currency format (e.g., £12.3456) if valid.
        /// Returns an '-' if input is null.
        /// </summary>
        public static string FormattedCurrencyValue(decimal? value)
        {
            if (value == null)
                return "-";

            return ConvertToCurrency(value.Value);
        }
    }
}
