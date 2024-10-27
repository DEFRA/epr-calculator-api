using Newtonsoft.Json;

namespace EPR.Calculator.API.Utils
{
    public static class CsvSanitiser
    {
        public static string SanitiseData<T>(T value)
        {
            var stringToSanitise = string.Empty;

            if (value == null)
            {
                return string.Empty;
            }

            if (value is object)
            {
                stringToSanitise = JsonConvert.SerializeObject(value);
            }

            stringToSanitise = value is object
                ? JsonConvert.SerializeObject(value)
                : value.ToString();

            stringToSanitise = stringToSanitise.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();

            stringToSanitise = stringToSanitise.Replace(",", string.Empty);

            return stringToSanitise;
        }
    }
}
