namespace EPR.Calculator.API.CommsCost
{
    using System.Globalization;
    using System.Text;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Generates the CommsCost report.
    /// </summary>
    /// <param name="DBContext">The database context.</param>
    public class CommsCostReportBuilder(ApplicationDBContext DBContext)
    {
        /// <summary>
        /// The database field for ProdRepHoPaWaT isn't implemented yet,
        /// so this method is a placeholder that returns a bunch of sample values
        /// and will be eventually replaced by data from the database.
        /// </summary>
        IDictionary<int, decimal> TempProdRepHoPaWaT { get; } = new Dictionary<int, decimal>
        {
            {0, 6980.000M},
            {1, 11850.000M},
            {2, 4900.000M},
        };

        /// <summary>
        /// The delimiter to use between fields in the report.
        /// </summary>
        private const char Delimiter = ',';

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
            "Producer Reported  Household Packaging Waste Tonnage",
            "Late Reporting Tonnage",
            "Producer Reported Household Tonnage + Late Reporting Tonnage",
            "Comms Cost - by Material Price Per Tonne",
        ];

        /// <summary>
        /// The country details we need for calculating the results.
        /// </summary>
        private IEnumerable<CountryDetails> Countries
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
        /// Generate the CommsCost report.
        /// </summary>
        /// <param name="totalValues">The total values, indexed by material ID.</param>
        /// <param name="lateReportingTonnage">
        /// The late reporting tonnage values, indexed by material ID.
        /// </param>
        /// <returns></returns>
        public string BuildReport(
            int runId,
            IDictionary<int, decimal> totalValues)
        {
            var byMaterialRecords = GetByMaterialThenCountry(runId, totalValues);

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

        private IEnumerable<CommsCostReportRecord> GetByMaterialThenCountry(
            int runId,
            IDictionary<int, decimal> totalValues)
        {
            var materials = GetMaterialDetails(runId);

            var records = materials.Select(material
                => BuildRecord(material, totalValues));

            return records;
        }

        private IEnumerable<MaterialDetails> GetMaterialDetails()
            => DBContext.Material.Select(material => new MaterialDetails
            {
                Id = material.Id,
                Name = material.Name,
                Code = material.Code,
            });

        private decimal GetLateReportingTonnage(int runId, string materialCode)
        {
            var parametersMaster = DBContext.CalculatorRuns
                .Single(run => run.Id == runId)
                .DefaultParameterSettingMaster
                ?? throw new InvalidOperationException("No parameters found.");
            return parametersMaster.Details
                .Single(d => d.ParameterUniqueReferenceId == $"LRET-{materialCode}")
                .ParameterValue;
        }

        private IEnumerable<MaterialDetails> GetMaterialDetails(int runId)
        {
            // TODO: The DefaultParameterSettingMaster can be null
            // - check what we should do when it is - are there default values to use instead?
            // Will throw an exception for now.
            var parameters = DBContext.CalculatorRuns
                .Single(run => run.Id == runId)
                .DefaultParameterSettingMaster?.Details
                ?? throw new InvalidOperationException("No parameters found.");

            // join the material and parameter tables using the material code
            // and select all the values we need.
            return DBContext.Material
                .Join(
                    parameters,
                    material => $"LRET-{material.Code}",
                    parameter => parameter.ParameterUniqueReferenceId,
                    (m,p)=> new MaterialDetails
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Code = m.Code,
                        LateReportingTonnage = p.ParameterValue
                    });
        }

        private CommsCostReportRecord BuildRecord(
            MaterialDetails material,
            IDictionary<int, decimal> totalValues)
            => new CommsCostReportRecord
            {
                Material = DBContext.Material.Single(m => m.Id == material.Id).Name,
                Total = totalValues[material.Id],
                PerCountryValues = CalculatePerCountryValue(Countries, totalValues[material.Id]),
                ProdRepHoPaWaT = TempProdRepHoPaWaT[material.Id],
                LateTonnageReporting = material.LateReportingTonnage,
            };

        private static IDictionary<int, decimal> CalculatePerCountryValue(
            IEnumerable<CountryDetails> countries,
            decimal totalCost) => countries.ToDictionary(
                country => country.Id,
                country => (totalCost/100) * country.Apportionment);


        private sealed record CommsCostReportRecord
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

            /// <summary>
            /// Gets the sum of the Producer Reported Household Packaging Waste Tonnage
            /// and the Late Tonnage Reporting
            /// </summary>
            public decimal PRHPAWTPlusLatTonRep => ProdRepHoPaWaT + LateTonnageReporting;

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
        /// For fetching only the required values from the country table.
        /// </summary>
        private struct CountryDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            public decimal Apportionment { get; init; }
        }

        /// <summary>
        /// For fetching only the required values from the materials table.
        /// </summary>
        private struct MaterialDetails
        {
            public int Id { get; init; }

            public string Name { get; init; }

            /// <summary>
            /// The code used as part of the key for retrieving the parameters.
            /// </summary>
            public string Code { get; init; }

            public decimal LateReportingTonnage { get; init; }
        }
    }
}
