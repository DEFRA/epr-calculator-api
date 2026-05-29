using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResultLateReportingTonnageJsonTests
    {
        [TestMethod]
        public void From_HandlesNullOrPopulated()
        {
            // Arrange
            var data = TestDataHelper.GetCalcResultLateReportingTonnage();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = CalcResultLateReportingTonnageJson.From(data, materials);

            // Assert
            Assert.IsNotNull(result);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "name": "Late Reporting Tonnage",
                "calcResultLateReportingTonnageDetails": [
                    {
                      "materialName": "Aluminium",
                      "totalLateReportingTonnage": 8000.000
                    },
                    {
                      "materialName": "Fibre composite",
                      "totalLateReportingTonnage": 10.000
                    },
                    {
                      "materialName": "Glass",
                      "totalLateReportingTonnage": 10.000
                    },
                    {
                      "materialName": "Paper or card",
                      "totalLateReportingTonnage": 0
                    },
                    {
                      "materialName": "Plastic",
                      "totalLateReportingTonnage": 2000.000
                    },
                    {
                      "materialName": "Steel",
                      "totalLateReportingTonnage": 0
                    },
                    {
                      "materialName": "Wood",
                      "totalLateReportingTonnage": 0
                    },
                    {
                      "materialName": "Other materials",
                      "totalLateReportingTonnage": 0
                    }
                ],
                "calcResultLateReportingTonnageTotal": 10020.000
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
