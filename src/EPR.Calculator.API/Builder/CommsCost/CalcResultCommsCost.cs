using System.Text;

namespace EPR.Calculator.API.Builder.CommsCost
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        /// <summary>
        /// The delimiter to use between fields in the report.
        /// </summary>
        public const char Delimiter = ',';

        /// <summary>
        /// Headers for the report - these go before the headers for each country.
        /// </summary>
        private IEnumerable<string> Headers1 { get; } =
        [
            "2a Comms Costs - by Material",
        ];

        /// <summary>
        /// Headers for the report - these go after the headers for each country.
        /// </summary>
        private IEnumerable<string> Headers2 { get; } =
        [
            "Total",
            "Producer Reported Household Packaging Waste Tonnage",
            "Late Reporting Tonnage",
            "Producer Reported Household Tonnage + Late Reporting Tonnage",
            "Comms Cost - by Material Price Per Tonne",
        ];

        public required IEnumerable<CalcResultCommsCostRecord> Records { get; init; }

        public required IEnumerable<string> CountryNames { get; init; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            // Append the column headers to the report.
            builder.AppendJoin(Delimiter, Headers1);
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, CountryNames);
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, Headers2);
            builder.Append(Environment.NewLine);

            // Append the records to the report.
            builder.AppendJoin(Environment.NewLine, Records);
            return builder.ToString();
        }
    }
}
