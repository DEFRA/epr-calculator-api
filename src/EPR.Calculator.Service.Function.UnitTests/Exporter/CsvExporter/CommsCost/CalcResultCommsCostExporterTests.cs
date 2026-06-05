using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.CommsCost
{
    [TestClass]
    public class CalcResultCommsCostExporterTests
    {
        private ICalcResultCommsCostExporter exporter = new CalcResultCommsCostExporter();

        [TestMethod]
        public void ExportCommsCost_PopulatedLists_ReturnsStringBuilderWithData()
        {
            // Arrange
            var communicationCost = new CalcResultCommsCost
            {
                OnePlusFourApportionment = new () { England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40 },
                ByMaterial = new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>
                {
                     ["AL"] = new() { Cost = new() { England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40 }, TotalCost = 100, HouseholdPackagingWasteTonnage = 50, PublicBinTonnage = 60, HouseholdDrinksContainersTonnage = 70, LateReportingTonnage = 80 }
                },
                CommsCostUkWide    = new ByCountryCost { England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40 },
                CommsCostByCountry = new ByCountryCost { England = 11, Wales = 21, Scotland = 31, NorthernIreland = 41 }
            };
            var materials = TestDataHelper.GetMaterialDetails();

            // Act
            var csvContent = new StringBuilder();
            exporter.Export(communicationCost, materials, csvContent);

            var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Parameters - Comms Costs" },
                new[] { null, "England","Wales","Scotland","Northern Ireland","Total" },
                new[] {"1 + 4 Apportionment %s","10.00000000%","20.00000000%","30.00000000%","40.00000000%","100.00000000%"},
                new string[] { },
                new[] { "2a Comms Costs - by Material","England","Wales","Scotland","Northern Ireland","Total","Producer Household Packaging Tonnage","Public Bin Tonnage","Household Drinks Containers Tonnage","Late Reporting Tonnage","Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage","Comms Cost - by Material Price Per Tonne" },
                new[] { "Aluminium","£10.00","£20.00","£30.00","£40.00","£100.00","50.000","60.000","70.000","80.000","260.000","£0.3846" },
                new[] { "Total"    ,"£10.00","£20.00","£30.00","£40.00","£100.00","50.000","60.000","70.000","80.000","260.000" },
                new string[] { },
                new[] { null, "England","Wales","Scotland","Northern Ireland","Total" },
                new [] {  "2b Comms Costs - UK wide"   ,"£10.00","£20.00","£30.00","£40.00","£100.00" },
                new [] {  "2c Comms Costs - by Country","£11.00","£21.00","£31.00","£41.00","£104.00" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
