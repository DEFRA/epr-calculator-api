namespace EPR.Calculator.API.CommsCost
{
    using EPR.Calculator.API.Data;
    using System.Globalization;
    using System.Text;

    public class CommsCostReportBuilder(ApplicationDBContext DBContext)
    {
        private const char Delimiter = ',';

        private IEnumerable<string> Headers1 { get; } =
        [
            "2a Comms Costs - by Material",
        ];

        private IEnumerable<string> Headers2 { get; } =
        [
            "Total",
            "Producer Reported  Household Packaging Waste Tonnage",
            "Late Reporting Tonnage",
            "Producer Reported Household Tonnage + Late Reporting Tonnage",
            "Comms Cost - by Material Price Per Tonne",
        ];



        private readonly Lazy<IEnumerable<CountryDetails>> _countries = new(
            ()=> from country in DBContext.Country
                join app in DBContext.CountryApportionment
                on country.Id equals app.CountryId
                select new CountryDetails
                {
                    Id = country.Id,
                    Name = country.Name,
                    Apportionment = app.Apportionment,
                });

        private IEnumerable<CountryDetails> Countries => _countries.Value;



        public string BuildReport()
        {
            var byMaterialRecords = GetByMaterialThenCountry();

            var builder = new StringBuilder();
            builder.AppendJoin(Delimiter, Headers1);
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, Countries.Select(c => c.Name));
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, Headers2);
            builder.Append(Environment.NewLine);
            builder.AppendJoin(Environment.NewLine, byMaterialRecords);
            var report = builder.ToString();

            return report;
        }

        public IEnumerable<CommsCostReportRecord> GetByMaterialThenCountry()
        {
            var materials = DBContext.Material.Select(material => new MaterialDetails
            {
                Id = material.Id,
                Name = material.Name,
                TotalValue = 2870.00M,
            });

            var records = materials.Select(BuildRecord);

            return records;
        }

        private CommsCostReportRecord BuildRecord(MaterialDetails material)
        {
            var countries = from country in DBContext.Country
                    join app in DBContext.CountryApportionment
                    on country.Id equals app.CountryId
                    select new CountryDetails
                    {
                        Id = country.Id,
                        Name = country.Name,
                        Apportionment = app.Apportionment,
                    };


            return new CommsCostReportRecord
            {
                Material = DBContext.Material.Single(m => m.Id == material.Id).Name,
                Total = material.TotalValue,
                PerCountryValues = CalculatePerCountryValue(countries, material.TotalValue),
                ProdRepHoPaWaT = 6980.000M,
                LateTonnageReporting = 8000.000M,
            };
        }

        private static IDictionary<int, decimal> CalculatePerCountryValue(
            IEnumerable<CountryDetails> countries,
            decimal totalCost) => countries.ToDictionary(
                country => country.Id,
                country => (totalCost/100) * country.Apportionment);


        public record CommsCostReportRecord
        {
            /// <summary>
            /// Gets the material this record is for.
            /// </summary>
            public required string Material { get; init; }

            /// <summary>
            /// Gets a dictionary of the values for each country.
            /// </summary>
            public required IDictionary<int, decimal> PerCountryValues { get; init; }

            public required decimal Total {  get; init; }

            /// <summary>
            /// Gets the Producer Reported Household Packaging Waste Tonnage.
            /// </summary>
            /// <remarks>
            /// TODO: Give this field a better name.
            /// </remarks>
            public required decimal ProdRepHoPaWaT { get; init; }

            public required decimal LateTonnageReporting { get; init; }

            public decimal PRHPAWTPlusLatTonRep => ProdRepHoPaWaT + LateTonnageReporting;

            public decimal PricePerTon => Total / ProdRepHoPaWaT;

            public override string ToString()
            {
                // Duplicate the currency format, and remove the commas seperating thousands, etc.
                // - otherwise they'll break our CSV file!
                var currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                currencyFormat.CurrencyGroupSeparator = string.Empty;

                // Price per tonne requires it's own formatter, as it's required to
                // display the value to 4 decimal places.
                var pricePerTonFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                pricePerTonFormat.CurrencyGroupSeparator = string.Empty;
                pricePerTonFormat.CurrencyDecimalDigits = 4;

                var fields = new List<string>
                {
                    Material,
                };
                fields.AddRange(PerCountryValues.Select(country => country.Value.ToString("C")));
                fields.AddRange(
                [
                    Total.ToString("C", currencyFormat),
                    ProdRepHoPaWaT.ToString("C", currencyFormat),
                    LateTonnageReporting.ToString("C", currencyFormat),
                    (ProdRepHoPaWaT + LateTonnageReporting).ToString("C", currencyFormat),
                    PricePerTon.ToString("C", currencyFormat),
                ]);

                return string.Join(Delimiter, fields);
            }
        }

        /// <summary>
        /// For fetching only the required values from the country table.
        /// </summary>
        private struct CountryDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            public decimal Apportionment { get; init; }
        }

        private struct MaterialDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            public decimal TotalValue { get; init; }
        }
    }
}
