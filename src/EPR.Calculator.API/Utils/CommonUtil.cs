namespace EPR.Calculator.API.Utils
{
    public static class CommonUtil
    {
        public static string ConvertToCalendarYear(string financialYear)
        {
            if (string.IsNullOrWhiteSpace(financialYear))
            {
                throw new ArgumentException("Financial year cannot be null or empty", nameof(financialYear));
            }

            var years = financialYear.Split('-');
            if (years.Length != 2 || !int.TryParse(years[0], out int startYear) || years[0].Length != 4 || years[1].Length != 2)
            {
                throw new FormatException("Financial year format is invalid. Expected format is 'YYYY-YY'.");
            }

            // For RPD data request, the calendar year is the year before the start of the financial year
            int calendarYear = startYear - 1;

            return calendarYear.ToString();
        }
    }
}