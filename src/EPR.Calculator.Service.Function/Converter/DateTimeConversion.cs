using System.Globalization;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Converter
{
    /// <summary>
    /// Converts a DateTime string value to DateFormatISO8601.
    /// </summary>
    public static class DateTimeConversion
    {
        public static string ConvertToIso8601Utc(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return string.Empty;
            }

            // Parse the input string
            DateTime parsedDate = DateTime.ParseExact(dateTimeString, CalculationResults.DateFormat, CultureInfo.InvariantCulture);

            DateTime utcDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            // Format as ISO 8601
            return utcDate.ToString(CalculationResults.DateFormatISO8601);

        }
    }
}
