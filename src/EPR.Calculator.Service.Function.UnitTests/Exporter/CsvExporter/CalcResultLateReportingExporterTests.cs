using System.Text;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
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
                        "Late Reporting Tonnage",
                        "Red + Red Medical Late Reporting Tonnage",
                        "Amber + Amber Medical Late Reporting Tonnage",
                        "Green + Green Medical Late Reporting Tonnage" },
                new[] { "Aluminium","1.230","2.340","3.450","4.560" },
                new[] { "Glass"     ,"1.340","2.450","3.560","4.670" },
                new[] { "Total"    ,"2.570","4.790","7.010","9.230" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
