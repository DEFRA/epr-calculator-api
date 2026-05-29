using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.PartialObligations
{
    [TestClass]
    public class CalcResultPartialObligationsExporterTests
    {
        private CalcResultPartialObligationsExporter exporter;
        private readonly List<MaterialDetail> materials = new List<MaterialDetail>()
        {
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
            new MaterialDetail { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" }
        };

        public CalcResultPartialObligationsExporterTests()
        {
            exporter = new CalcResultPartialObligationsExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludePartialObligationWithModulation()
        {
            var showModulation = true;
            var materials = GetMaterials();
            var projectedProducers = new CalcResultPartialObligations()
            {
                PartialObligations = GetCalcResultPartialObligationsListWithRam()
            };

            var csvContent = new StringBuilder();

            exporter.Export(projectedProducers, materials, csvContent, showModulation);
            var rows = CsvTestUtils.GetRows(csvContent);

            Assert.IsTrue(rows[2][0].Contains(CalcResultPartialObligationHeaders.PartialObligations));

            var materialHeaders = rows[4];
            var columnHeaders = rows[5];
            var columnValues = rows[6];

            var materialHeadersIndexes = CsvTestUtils.FindAllHeaderIndexes(columnHeaders, CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage);
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[0]].Contains("Aluminium Breakdown"));
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[1]].Contains("Glass Breakdown"));
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[2]].Contains("Other materials Breakdown"));

            var data = CsvTestUtils.GetColumnHeaderValues(columnHeaders, columnValues);

            Assert.AreEqual("101001", data[CalcResultPartialObligationHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, data[CalcResultPartialObligationHeaders.SubsidiaryId].First());
            Assert.AreEqual("Allied Packaging", data[CalcResultPartialObligationHeaders.ProducerOrSubsidiaryName].First());
            Assert.AreEqual("", data[CalcResultPartialObligationHeaders.TradingName].First());
            Assert.AreEqual("1", data[CalcResultPartialObligationHeaders.Level].First());
            Assert.AreEqual("2024", data[CalcResultPartialObligationHeaders.SubmissionYear].First());
            Assert.AreEqual("366", data[CalcResultPartialObligationHeaders.DaysInSubmissionYear].First());
            Assert.AreEqual("15/07/2024", data[CalcResultPartialObligationHeaders.JoiningDate].First());
            Assert.AreEqual("183", data[CalcResultPartialObligationHeaders.ObligatedDays].First());
            Assert.AreEqual("50.00%", data[CalcResultPartialObligationHeaders.ObligatedPercentage].First());

            //Aluminium
            Assert.AreEqual("100.000", data[CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage][0]);
            Assert.AreEqual("1.000", data[CalcResultPartialObligationHeaders.HouseholdRedTonnage][0]);
            Assert.AreEqual("2.000", data[CalcResultPartialObligationHeaders.HouseholdAmberTonnage][0]);
            Assert.AreEqual("3.000", data[CalcResultPartialObligationHeaders.HouseholdGreenTonnage][0]);
            Assert.AreEqual("4.000", data[CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage][0]);
            Assert.AreEqual("5.000", data[CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage][0]);
            Assert.AreEqual("6.000", data[CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage][0]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PublicBinTonnage][0]);
            Assert.AreEqual("7.000", data[CalcResultPartialObligationHeaders.PublicBinRedTonnage][0]);
            Assert.AreEqual("8.000", data[CalcResultPartialObligationHeaders.PublicBinAmberTonnage][0]);
            Assert.AreEqual("9.000", data[CalcResultPartialObligationHeaders.PublicBinGreenTonnage][0]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage][0]);
            Assert.AreEqual("11.000", data[CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage][0]);
            Assert.AreEqual("12.000", data[CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage][0]);
            Assert.AreEqual("120.000", data[CalcResultPartialObligationHeaders.TotalTonnage][0]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage][0]);
            Assert.AreEqual("50.000", data[CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage][0]);
            Assert.AreEqual("13.000", data[CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage][0]);
            Assert.AreEqual("14.000", data[CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage][0]);
            Assert.AreEqual("15.000", data[CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage][0]);
            Assert.AreEqual("16.000", data[CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage][0]);
            Assert.AreEqual("17.000", data[CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage][0]);
            Assert.AreEqual("18.000", data[CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage][0]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PartialPublicBinTonnage][0]);
            Assert.AreEqual("19.000", data[CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage][0]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage][0]);
            Assert.AreEqual("21.000", data[CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage][0]);
            Assert.AreEqual("22.000", data[CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage][0]);
            Assert.AreEqual("23.000", data[CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage][0]);
            Assert.AreEqual("24.000", data[CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage][0]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.PartialTotalTonnage][0]);
            Assert.AreEqual("30.000", data[CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage][0]);
            //Glass
            Assert.AreEqual("100.000", data[CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage][1]);
            Assert.AreEqual("1.000", data[CalcResultPartialObligationHeaders.HouseholdRedTonnage][1]);
            Assert.AreEqual("2.000", data[CalcResultPartialObligationHeaders.HouseholdAmberTonnage][1]);
            Assert.AreEqual("3.000", data[CalcResultPartialObligationHeaders.HouseholdGreenTonnage][1]);
            Assert.AreEqual("4.000", data[CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage][1]);
            Assert.AreEqual("5.000", data[CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage][1]);
            Assert.AreEqual("6.000", data[CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage][1]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PublicBinTonnage][1]);
            Assert.AreEqual("7.000", data[CalcResultPartialObligationHeaders.PublicBinRedTonnage][1]);
            Assert.AreEqual("8.000", data[CalcResultPartialObligationHeaders.PublicBinAmberTonnage][1]);
            Assert.AreEqual("9.000", data[CalcResultPartialObligationHeaders.PublicBinGreenTonnage][1]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage][1]);
            Assert.AreEqual("11.000", data[CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage][1]);
            Assert.AreEqual("12.000", data[CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage][1]);
            Assert.AreEqual("70.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnage][0]);
            Assert.AreEqual("13.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedTonnage][0]);
            Assert.AreEqual("14.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberTonnage][0]);
            Assert.AreEqual("15.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenTonnage][0]);
            Assert.AreEqual("16.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedMedicalTonnage][0]);
            Assert.AreEqual("17.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberMedicalTonnage][0]);
            Assert.AreEqual("18.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenMedicalTonnage][0]);
            Assert.AreEqual("190.000", data[CalcResultPartialObligationHeaders.TotalTonnage][1]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage][1]);
            Assert.AreEqual("50.000", data[CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage][1]);
            Assert.AreEqual("19.000", data[CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage][1]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage][1]);
            Assert.AreEqual("21.000", data[CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage][1]);
            Assert.AreEqual("22.000", data[CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage][1]);
            Assert.AreEqual("23.000", data[CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage][1]);
            Assert.AreEqual("24.000", data[CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage][1]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PartialPublicBinTonnage][1]);
            Assert.AreEqual("25.000", data[CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage][1]);
            Assert.AreEqual("26.000", data[CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage][1]);
            Assert.AreEqual("27.000", data[CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage][1]);
            Assert.AreEqual("28.000", data[CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage][1]);
            Assert.AreEqual("29.000", data[CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage][1]);
            Assert.AreEqual("30.000", data[CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage][1]);
            Assert.AreEqual("30.000", data[CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage][1]);
            Assert.AreEqual("35.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnage][0]);
            Assert.AreEqual("31.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedTonnage][0]);
            Assert.AreEqual("32.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberTonnage][0]);
            Assert.AreEqual("33.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenTonnage][0]);
            Assert.AreEqual("34.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedMedicalTonnage][0]);
            Assert.AreEqual("35.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberMedicalTonnage][0]);
            Assert.AreEqual("36.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenMedicalTonnage][0]);
            Assert.AreEqual("95.000", data[CalcResultPartialObligationHeaders.PartialTotalTonnage][1]);
        }

        [TestMethod]
        public void Export_ShouldIncludePartialObligationWithoutModulation()
        {
            var showModulation = false;
            var materials = GetMaterials();
            var projectedProducers = new CalcResultPartialObligations()
            {
                PartialObligations = GetCalcResultPartialObligationsList()
            };

            var csvContent = new StringBuilder();

            exporter.Export(projectedProducers, materials, csvContent, showModulation);
            var rows = CsvTestUtils.GetRows(csvContent);

            Assert.IsTrue(rows[2][0].Contains(CalcResultPartialObligationHeaders.PartialObligations));

            var materialHeaders = rows[4];
            var columnHeaders = rows[5];
            var columnValues = rows[6];

            var materialHeadersIndexes = CsvTestUtils.FindAllHeaderIndexes(columnHeaders, CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage);
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[0]].Contains("Aluminium Breakdown"));
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[1]].Contains("Glass Breakdown"));
            Assert.IsTrue(materialHeaders[materialHeadersIndexes[2]].Contains("Other materials Breakdown"));

            var data = CsvTestUtils.GetColumnHeaderValues(columnHeaders, columnValues);

            Assert.AreEqual("101001", data[CalcResultPartialObligationHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, data[CalcResultPartialObligationHeaders.SubsidiaryId].First());
            Assert.AreEqual("Allied Packaging", data[CalcResultPartialObligationHeaders.ProducerOrSubsidiaryName].First());
            Assert.AreEqual("", data[CalcResultPartialObligationHeaders.TradingName].First());
            Assert.AreEqual("1", data[CalcResultPartialObligationHeaders.Level].First());
            Assert.AreEqual("2024", data[CalcResultPartialObligationHeaders.SubmissionYear].First());
            Assert.AreEqual("366", data[CalcResultPartialObligationHeaders.DaysInSubmissionYear].First());
            Assert.AreEqual("15/07/2024", data[CalcResultPartialObligationHeaders.JoiningDate].First());
            Assert.AreEqual("183", data[CalcResultPartialObligationHeaders.ObligatedDays].First());
            Assert.AreEqual("50.00%", data[CalcResultPartialObligationHeaders.ObligatedPercentage].First());

            //Aluminium
            Assert.AreEqual("100.000", data[CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage][0]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PublicBinTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage][0]);
            Assert.AreEqual("120.000", data[CalcResultPartialObligationHeaders.TotalTonnage][0]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage][0]);
            Assert.AreEqual("50.000", data[CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage][0]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PartialPublicBinTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage][0]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.PartialTotalTonnage][0]);
            Assert.AreEqual("30.000", data[CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage][0]);
            //Glass
            Assert.AreEqual("100.000", data[CalcResultPartialObligationHeaders.HouseholdPackagingWasteTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdRedTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdAmberTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdGreenTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdRedMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdAmberMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdGreenMedicalTonnage][1]);
            Assert.AreEqual("20.000", data[CalcResultPartialObligationHeaders.PublicBinTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinRedTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinAmberTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinGreenTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinRedMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinAmberMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PublicBinGreenMedicalTonnage][1]);
            Assert.AreEqual("70.000", data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.HouseholdDrinksContainersGreenMedicalTonnage][0]);
            Assert.AreEqual("190.000", data[CalcResultPartialObligationHeaders.TotalTonnage][1]);
            Assert.AreEqual("60.000", data[CalcResultPartialObligationHeaders.SelfManagedConsumerWasteTonnage][1]);
            Assert.AreEqual("50.000", data[CalcResultPartialObligationHeaders.PartialHouseholdPackagingWasteTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdRedTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdAmberTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdGreenTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdRedMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdAmberMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdGreenMedicalTonnage][1]);
            Assert.AreEqual("10.000", data[CalcResultPartialObligationHeaders.PartialPublicBinTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinRedTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinAmberTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinGreenTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinRedMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinAmberMedicalTonnage][1]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialPublicBinGreenMedicalTonnage][1]);
            Assert.AreEqual("30.000", data[CalcResultPartialObligationHeaders.PartialSelfManagedConsumerWasteTonnage][1]);
            Assert.AreEqual("35.000", data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersRedMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersAmberMedicalTonnage][0]);
            Should.Throw<KeyNotFoundException>(() => data[CalcResultPartialObligationHeaders.PartialHouseholdDrinksContainersGreenMedicalTonnage][0]);
            Assert.AreEqual("95.000", data[CalcResultPartialObligationHeaders.PartialTotalTonnage][1]);
        }

        [TestMethod]
        public void Export_ShouldHandleWhenEmpty()
        {
            var showModulation = true;
            var materials = GetMaterials();
            var projectedProducers = new CalcResultPartialObligations()
            {
                PartialObligations = []
            };

            var csvContent = new StringBuilder();

            exporter.Export(projectedProducers, materials, csvContent, showModulation);
            var rows = CsvTestUtils.GetRows(csvContent);

            Assert.IsTrue(rows[2][0].Contains(CalcResultPartialObligationHeaders.PartialObligations));
            Assert.IsTrue(rows[6][0].Contains(CalcResultPartialObligationHeaders.NoPartialObligations));
        }

        private ImmutableList<CalcResultPartialObligation> GetCalcResultPartialObligationsListWithRam()
        {
            return
            [
                new CalcResultPartialObligation
                {
                    ProducerId = 101001,
                    ProducerName = "Allied Packaging",
                    DaysObligated = 183,
                    DaysInSubmissionYear = 366,
                    Level = "1",
                    JoiningDate = "15/07/2024",
                    ObligatedFactor = 0.5m,
                    SubmissionYear = 2024,
                    SubsidiaryId = null,
                    PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>
                    {
                        {
                            "AL",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage()
                                {
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                PublicBinTonnage = 20,
                                PublicBinRAMTonnage = new RAMTonnage()
                                {
                                    RedTonnage = 7, AmberTonnage = 8, GreenTonnage = 9, RedMedicalTonnage = 10, AmberMedicalTonnage = 11, GreenMedicalTonnage = 12
                                },
                                TotalTonnage = 120,
                                SelfManagedConsumerWasteTonnage = 60,
                                PartialHouseholdTonnage = 50,
                                PartialHouseholdRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 13, AmberTonnage = 14, GreenTonnage = 15, RedMedicalTonnage = 16, AmberMedicalTonnage = 17, GreenMedicalTonnage = 18
                                },
                                PartialPublicBinTonnage = 10,
                                PartialPublicBinRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 19, AmberTonnage = 20, GreenTonnage = 21, RedMedicalTonnage = 22, AmberMedicalTonnage = 23, GreenMedicalTonnage = 24
                                },
                                PartialTotalTonnage = 60,
                                PartialSelfManagedConsumerWasteTonnage = 30
                            }
                        },
                        {
                            "GL",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                PublicBinTonnage = 20,
                                PublicBinRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 7, AmberTonnage = 8, GreenTonnage = 9, RedMedicalTonnage = 10, AmberMedicalTonnage = 11, GreenMedicalTonnage = 12
                                },
                                HouseholdDrinksContainersTonnage = 70,
                                HouseholdDrinksContainersRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 13, AmberTonnage = 14, GreenTonnage = 15, RedMedicalTonnage = 16, AmberMedicalTonnage = 17, GreenMedicalTonnage = 18
                                },
                                TotalTonnage = 190,
                                SelfManagedConsumerWasteTonnage = 60,
                                PartialHouseholdTonnage = 50,
                                PartialHouseholdRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 19, AmberTonnage = 20, GreenTonnage = 21, RedMedicalTonnage = 22, AmberMedicalTonnage = 23, GreenMedicalTonnage = 24
                                },
                                PartialPublicBinTonnage = 10,
                                PartialPublicBinRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 25, AmberTonnage = 26, GreenTonnage = 27, RedMedicalTonnage = 28, AmberMedicalTonnage = 29, GreenMedicalTonnage = 30
                                },
                                PartialHouseholdDrinksContainersTonnage = 35,
                                PartialHouseholdDrinksContainersRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 31, AmberTonnage = 32, GreenTonnage = 33, RedMedicalTonnage = 34, AmberMedicalTonnage = 35, GreenMedicalTonnage = 36
                                },
                                PartialTotalTonnage = 95,
                                PartialSelfManagedConsumerWasteTonnage = 30
                            }
                        },
                        {
                            "OT",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 50,
                                HouseholdRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                PublicBinTonnage = 10,
                                PublicBinRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                TotalTonnage = 60,
                                SelfManagedConsumerWasteTonnage = 20,
                                PartialHouseholdTonnage = 25,
                                PartialHouseholdRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                PartialPublicBinTonnage = 5,
                                PartialPublicBinRAMTonnage = new RAMTonnage(){
                                    RedTonnage = 1, AmberTonnage = 2, GreenTonnage = 3, RedMedicalTonnage = 4, AmberMedicalTonnage = 5, GreenMedicalTonnage = 6
                                },
                                PartialTotalTonnage = 30,
                                PartialSelfManagedConsumerWasteTonnage = 10
                            }
                        }
                    }
                }
            ];
        }

        private ImmutableList<CalcResultPartialObligation> GetCalcResultPartialObligationsList()
        {
            return new List<CalcResultPartialObligation>
            {
                new CalcResultPartialObligation
                {
                    ProducerId = 101001,
                    ProducerName = "Allied Packaging",
                    DaysObligated = 183,
                    DaysInSubmissionYear = 366,
                    Level = "1",
                    JoiningDate = "15/07/2024",
                    ObligatedFactor = 0.5m,
                    SubmissionYear = 2024,
                    SubsidiaryId = null,
                    PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>
                    {
                        {
                            "AL",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 100,
                                PublicBinTonnage = 20,
                                TotalTonnage = 120,
                                SelfManagedConsumerWasteTonnage = 60,
                                PartialHouseholdTonnage = 50,
                                PartialPublicBinTonnage = 10,
                                PartialTotalTonnage = 60,
                                PartialSelfManagedConsumerWasteTonnage = 30
                            }
                        },
                        {
                            "GL",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 100,
                                PublicBinTonnage = 20,
                                HouseholdDrinksContainersTonnage = 70,
                                TotalTonnage = 190,
                                SelfManagedConsumerWasteTonnage = 60,
                                PartialHouseholdTonnage = 50,
                                PartialPublicBinTonnage = 10,
                                PartialHouseholdDrinksContainersTonnage = 35,
                                PartialTotalTonnage = 95,
                                PartialSelfManagedConsumerWasteTonnage = 30
                            }
                        },
                        {
                            "OT",
                            new CalcResultPartialObligationTonnage
                            {
                                HouseholdTonnage = 50,
                                PublicBinTonnage = 10,
                                TotalTonnage = 60,
                                SelfManagedConsumerWasteTonnage = 20,
                                PartialHouseholdTonnage = 25,
                                PartialPublicBinTonnage = 5,
                                PartialTotalTonnage = 30,
                                PartialSelfManagedConsumerWasteTonnage = 10
                            }
                        }
                    }
                }
            }.ToImmutableList();
        }

        private ImmutableList<MaterialDetail> GetMaterials()
        {
            return new List<MaterialDetail>
            {
                new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
                new MaterialDetail { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" },
            }.ToImmutableList();
        }
    }
}
