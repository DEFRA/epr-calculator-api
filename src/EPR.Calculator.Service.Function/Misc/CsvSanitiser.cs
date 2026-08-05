using System.Globalization;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Misc
{
    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value, bool appendLrmCharacterToPreventRenderedAsFormula = false)
        {
            if (value is null)
                return CommonConstants.CsvFileDelimiter;

            var stringToSanitise = value
                .ToString()?
                .Replace(Environment.NewLine             , string.Empty)
                .Replace(CommonConstants.TabSpace        , string.Empty)
                .Replace(CommonConstants.CsvFileDelimiter, string.Empty)
                .Trim() ?? string.Empty;

            if (appendLrmCharacterToPreventRenderedAsFormula &&
                stringToSanitise.Length > 0 &&
                stringToSanitise != CommonConstants.Hyphen)
            {
                stringToSanitise = "\u200E" + stringToSanitise; // LEFT-TO-RIGHT MARK
            }

            // Apply the speech marks to handle the comma in the text
            return $"{CommonConstants.DoubleQuote}{stringToSanitise}{CommonConstants.DoubleQuote}{CommonConstants.CsvFileDelimiter}";
        }

        public static string SanitiseData(
            decimal? value,
            DecimalPlaces? roundTo,
            DecimalFormats? valueFormat,
            bool isCurrency = false,
            bool isPercentage = false,
            bool canBeEmpty = false)
        {
            if (canBeEmpty && value is null)
                return $"{CommonConstants.DoubleQuote}{CommonConstants.Hyphen}{CommonConstants.DoubleQuote}{CommonConstants.CsvFileDelimiter}";

            var decimalValue = value.GetValueOrDefault();

            string formattedValue;

            if (isCurrency)
            {
                formattedValue = FormatUtils.FormatCurrency(decimalValue, ((int?)roundTo) ?? 2);
            }
            else if (isPercentage)
            {
                formattedValue = FormatUtils.FormatPercentage(decimalValue, ((int?)roundTo) ?? 8);
            }
            else
            {
                var roundedValue = roundTo == null
                    ? decimalValue
                    : MathUtils.RoundAwayFromZero(decimalValue, (int)roundTo);

                formattedValue = valueFormat == null
                    ? roundedValue.ToString(CultureInfo.InvariantCulture)
                    : roundedValue.ToString(valueFormat.ToString(), CultureInfo.InvariantCulture);
            }

            // Apply the speech marks to handle the comma in the currency
            return $"{CommonConstants.DoubleQuote}{formattedValue}{CommonConstants.DoubleQuote}{CommonConstants.CsvFileDelimiter}";
        }
    }
}
