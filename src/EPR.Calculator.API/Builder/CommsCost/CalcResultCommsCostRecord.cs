using System.Globalization;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public class CalcResultCommsCostRecord
    {
        public CalcResultCommsCostRecord(MaterialDetails material, IEnumerable<CountryDetails> countries)
        {
            Material = material.Name;
            Total = material.TotalValue;
            PerCountryValues = countries.ToDictionary(
                country => country.Id,
                country => material.TotalValue / 100 * country.Apportionment);
            ProdRepHoPaWaT = material.ProdRepHoPaWaT;
            LateTonnageReporting = material.LateReportingTonnage;
        }

        /// <summary>
        /// Gets the material this record is for.
        /// </summary>
        public string Material { get; }

        /// <summary>
        /// Gets a dictionary of the values for each country.
        /// </summary>
        public IDictionary<int, decimal> PerCountryValues { get; }

        public decimal Total { get; }

        /// <summary>
        /// Gets the Producer Reported Household Packaging Waste Tonnage.
        /// </summary>
        /// <remarks>
        /// TODO: Give this field a better name.
        /// </remarks>
        public decimal ProdRepHoPaWaT { get; }

        public decimal LateTonnageReporting { get; }

        /// <summary>
        /// Gets the sum of the Producer Reported Household Packaging Waste Tonnage
        /// and the Late Tonnage Reporting
        /// </summary>
        public decimal PRHPAWTPlusLatTonRep => ProdRepHoPaWaT + LateTonnageReporting;

        /// <summary>
        /// The price per ton.
        /// </summary>
        public decimal PricePerTon => Total / PRHPAWTPlusLatTonRep;

        /// <inheritdoc/>
        public override string ToString()
        {
            // Duplicate the currency format, and remove the commas seperating thousands,
            // otherwise they'll break our CSV file!
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            var currencyFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
            currencyFormat.CurrencyGroupSeparator = string.Empty;
            currencyFormat.NumberDecimalDigits = 2;

            var fields = new List<string>
                {
                    Material,
                };
            fields.AddRange(PerCountryValues.Select(country => country.Value.ToString("C", currencyFormat)));
            fields.AddRange(
            [
                Total.ToString("C", currencyFormat),
                    ProdRepHoPaWaT.ToString("F3"),
                    LateTonnageReporting.ToString("F3"),
                    (ProdRepHoPaWaT + LateTonnageReporting).ToString("F3"),
                    PricePerTon.ToString("C4", culture),
                ]);

            return string.Join(CalcResultCommsCost.Delimiter, fields);
        }
    }
}
