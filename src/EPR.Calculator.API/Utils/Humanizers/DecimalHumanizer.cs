using System.Globalization;

namespace EPR.Calculator.API.Utils.Humanizers;

public static class DecimalHumanizer
{
    extension(decimal value)
    {
        /// <summary>
        ///     Returns a humanized string value of the decimal for display. The formatting is determined by
        ///     <see cref="unit" /> which supports several special formatting cases (<c>C</c> for currency, <c>%</c> for
        ///     percentage, <c>t</c> for tonnage).
        /// </summary>
        /// <param name="unit">
        ///     Output unit type. This can be any string and generally will be suffixed to the display value.
        /// </param>
        /// <param name="decimals">
        ///     Allows overriding the number of decimal places to display.
        ///     If unspecified, it is determined per <see cref="unit" /> with a fallback of <c>3</c>.
        /// </param>
        /// <param name="rounding">
        ///     Allows overriding the <see cref="MidpointRounding" /> method to use for rounding.
        ///     If unspecified, it is determined per <see cref="unit" /> with a fallback of
        ///     <see cref="MidpointRounding.ToZero" />.
        /// </param>
        /// <param name="culture">
        ///     An object that supplies culture-specific formatting information.
        ///     If unspecified, uses <see cref="CultureInfo.CurrentCulture" />.
        /// </param>
        /// <example>
        ///     <c>1000.0099m.Humanize("C")</c> returns <c>£1,000.00</c><br />
        ///     <c>1000.0000m.Humanize("%")</c> returns <c>1,000%</c><br />
        ///     <c>1000.1239m.Humanize("t")</c> returns <c>1,000.123 t</c>
        /// </example>
        public string Humanize(
            string? unit = null,
            int? decimals = null,
            MidpointRounding? rounding = null,
            CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            if (unit is "C" or "c")
                return value.HumanizeCurrency(decimals, rounding, culture);

            var decimalPlaces = decimals ?? (unit is "%" ? 2 : 3);
            var rounded = Math.Round(value, decimalPlaces, rounding ?? MidpointRounding.ToZero);

            var numericFormat = rounded % 1m != 0m
                ? $"#,##0.{new string('0', decimalPlaces)}"
                : "#,##0";

            var suffix = unit switch
            {
                "%" => "\\" + culture.NumberFormat.PercentSymbol,
                "T" or "t" => " \\t",
                null or "" => "",
                _ => "\\" + unit
            };

            return rounded.ToString(numericFormat + suffix, culture);
        }

        private string HumanizeCurrency(int? decimals, MidpointRounding? rounding, CultureInfo culture)
        {
            var rounded = Math.Round(
                value,
                decimals ?? culture.NumberFormat.CurrencyDecimalDigits,
                rounding ?? MidpointRounding.ToZero);

            return rounded.ToString("C", culture);
        }
    }
}
