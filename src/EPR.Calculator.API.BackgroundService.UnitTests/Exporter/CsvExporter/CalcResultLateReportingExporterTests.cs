using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultLateReportingExporterTests
    {
        private CalcResultLateReportingExporter exporter;

        public CalcResultLateReportingExporterTests()
        {
            exporter = new CalcResultLateReportingExporter();
        }

        /// <summary>
        /// Checks that the output matches the expected format.
        /// </summary>
        [TestMethod]
        public void CanCallPrepareData()
        {
            // Arrange
            var calcResultLateReportingData = new CalcResultLateReportingTonnage()
            {
                ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
                {
                    ["AL"] = new() { Total = 1.23m, Red = 2.34m, Amber = 3.45m, Green = 4.56m },
                    ["GL"] = new() { Total = 1.34m, Red = 2.45m, Amber = 3.56m, Green = 4.67m }
                }
            };
            var materials = TestDataHelper.GetMaterialDetails();


            var csvContent = new StringBuilder();

            // Act
            exporter.Export(calcResultLateReportingData, materials, csvContent);
            var result = csvContent.ToString().ReplaceLineEndings("\n").Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Parameters - Late Reporting Tonnages" },
                new[] { "Material",
                        "Red + Red Medical Late Reporting Tonnage",
                        "Amber + Amber Medical Late Reporting Tonnage",
                        "Green + Green Medical Late Reporting Tonnage",
                        "Total Late Reporting Tonnage" },
                new[] { "Aluminium","2.340","3.450","4.560","1.230" },
                new[] { "Glass"     ,"2.450","3.560","4.670","1.340" },
                new[] { "Total"    ,"4.790","7.010","9.230","2.570" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
