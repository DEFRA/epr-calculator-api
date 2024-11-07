namespace EPR.Calculator.API.CommsCost
{
    using System.Globalization;
    using System.Text;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;

    /// <summary>
    /// Generates the CommsCost report.
    /// </summary>
    /// <param name="DBContext">The database context.</param>
    public class CommsCostReportBuilder(ApplicationDBContext DBContext)
    {
        /// <summary>
        /// The delimiter to use between fields in the report.
        /// </summary>
        private const char Delimiter = ',';

        /// <summary>
        /// The key used to identify household records in the producer_reported_material table.
        /// </summary>
        private const string HouseHoldIndicator = "HH";

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

        /// <summary>
        /// Generate the CommsCost report.
        /// </summary>
        /// <param name="totalValues">The total values, indexed by material ID.</param>
        /// <param name="lateReportingTonnage">
        /// The late reporting tonnage values, indexed by material ID.
        /// </param>
        /// <returns></returns>
        public string BuildReport(int runId)
        {
            var countries = GetCountryDetails();
            var materials = GetMaterialDetails(runId);
            var records = materials.Select(material => new CommsCostReportRecord(material, countries));

            var builder = new StringBuilder();

            // Append the column headers to the report.
            builder.AppendJoin(Delimiter, Headers1);
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, countries.Select(c => c.Name));
            builder.Append(Delimiter);
            builder.AppendJoin(Delimiter, Headers2);
            builder.Append(Environment.NewLine);

            // Append the records to the report.
            builder.AppendJoin(Environment.NewLine, records);
            var report = builder.ToString();

            return report;
        }

        /// <summary>
        /// Retrieve the materials data from the database.
        /// </summary>
        private IEnumerable<MaterialDetails> GetMaterialDetails(int runId)
        {
            var parameters = DBContext.CalculatorRuns
                .Single(run => run.Id == runId)
                .DefaultParameterSettingMaster?.Details
                ?? throw new InvalidOperationException("No parameters found.");

            // Select the materials details from the database.
            return DBContext.Material
                .Select((material, parameter) => new MaterialDetails
                {
                    Id = material.Id,
                    Name = material.Name,
                    TotalValue = parameters.Single(
                        p => p.ParameterUniqueReferenceId == $"COMC-{material.Code}").ParameterValue,
                    LateReportingTonnage = parameters.Single(
                        p => p.ParameterUniqueReferenceId == $"LRET-{material.Code}").ParameterValue,
                    ProdRepHoPaWaT = CalculateProdRepHoPaWaT(material)
                });
        }

        /// <summary>
        /// Retrieve the country details from the database.
        /// </summary>
        private IEnumerable<CountryDetails> GetCountryDetails()
            => from country in DBContext.Country
               join app in DBContext.CountryApportionment
               on country.Id equals app.CountryId
               select new CountryDetails
               {
                   Id = country.Id,
                   Name = country.Name,
                   Apportionment = app.Apportionment,
               };

        /// <summary>
        /// Calculates the Producer Reported Household Packaging Waste Tonnage
        /// by summing the household tonnage reported for the given material by all the producers.
        /// </summary>
        private static decimal CalculateProdRepHoPaWaT(Material material)
            => material.ProducerReportedMaterials
                .Where(m => m.PackagingType == HouseHoldIndicator)
                .Sum(m => m.PackagingTonnage);

        /// <summary>
        /// A record in the CommsCost report.
        /// </summary>
        /// <remarks>
        /// Currently only used internally to <see cref="CommsCostReportBuilder"/>,
        /// since the report is returned as a string.
        /// We could return these records directly instead,
        /// if another class needs to have controll over the formatting.
        /// </remarks>
        private sealed record CommsCostReportRecord
        {
            public CommsCostReportRecord(MaterialDetails material, IEnumerable<CountryDetails> countries)
            {
                Material = material.Name;
                Total = material.TotalValue;
                PerCountryValues = countries.ToDictionary(
                    country => country.Id,
                    country => (material.TotalValue / 100) * country.Apportionment);
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
                var currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
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
                    PricePerTon.ToString("C4"),
                ]);

                return string.Join(Delimiter, fields);
            }
        }

        /// <summary>
        /// For use in database selects, to only retrieve the neccesary values.
        /// </summary>
        private struct CountryDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            public decimal Apportionment { get; init; }
        }

        /// <summary>
        /// For use in database selects, to only retrieve the neccesary values.
        /// </summary>
        private struct MaterialDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            public decimal TotalValue { get; init; }

            public decimal LateReportingTonnage { get; init; }

            public decimal ProdRepHoPaWaT { get; init; }
        }
    }
}