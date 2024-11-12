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
    public class CalcResultCommsCostBuilder(ApplicationDBContext DBContext) : ICalcResultCommsCostBuilder
    {
        /// <summary>
        /// The key used to identify household records in the producer_reported_material table.
        /// </summary>
        private const string HouseHoldIndicator = "HH";



        /// <summary>
        /// Generate the CommsCost report.
        /// </summary>
        /// <param name="totalValues">The total values, indexed by material ID.</param>
        /// <param name="lateReportingTonnage">
        /// The late reporting tonnage values, indexed by material ID.
        /// </param>
        /// <returns></returns>
        public CalcResultCommsCost Construct(int runId)
        {
            var countries = GetCountryDetails();
            var materials = GetMaterialDetails(runId);
            var records = materials.Select(material => new CalcResultCommsCostRecord(material, countries));

            var report = new CalcResultCommsCost
            {
                Records = records,
                CountryNames = countries.Select(c => c.Name)
            };

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
    }
}