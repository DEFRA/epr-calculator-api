using Newtonsoft.Json;

namespace EPR.Calculator.API.Utils
{
    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value)
        {
            if (value == null) return string.Empty;

            // If the value is a string, use it directly; otherwise, serialize the object to JSON.
            var stringToSanitise = value is string
                ? value.ToString()
                : JsonConvert.SerializeObject(value);

            // Remove newline, carriage returns, and commas, then trim
            stringToSanitise = stringToSanitise.Replace("\r", string.Empty)
                                               .Replace("\n", string.Empty)
                                               .Replace(",", string.Empty)
                                               .Trim();

            return stringToSanitise;
        }
    }
}
