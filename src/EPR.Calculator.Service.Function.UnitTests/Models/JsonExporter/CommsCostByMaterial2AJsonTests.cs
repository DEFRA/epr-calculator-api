using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CommsCostByMaterial2AJsonTests
    {
        [TestMethod]
        public void From_MapsDictionaryToMaterialBreakdown_AndSetsGlassDrinksContainers()
        {
            // Arrange
            var materials = TestDataHelper.GetMaterialDetails();

            var comms = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
            {
                ["AL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 100.25m,
                    PublicBinTonnage = 50.50m,
                    TotalReportedTonnage = 150.75m,
                    HouseholdDrinksContainersTonnage = 0m,
                    PriceperTonne = 0.42m,
                    ProducerTotalCostWithoutBadDebtProvision = 10m,
                    BadDebtProvision = 1m,
                    ProducerTotalCostwithBadDebtProvision = 11m,
                    EnglandWithBadDebtProvision = 4m,
                    WalesWithBadDebtProvision = 3m,
                    ScotlandWithBadDebtProvision = 2m,
                    NorthernIrelandWithBadDebtProvision = 1m,
                },
                ["GL"] = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 200m,
                    PublicBinTonnage = 80m,
                    TotalReportedTonnage = 280m,
                    HouseholdDrinksContainersTonnage = 12.34m,
                    PriceperTonne = 0.3m,
                    ProducerTotalCostWithoutBadDebtProvision = 20m,
                    BadDebtProvision = 2m,
                    ProducerTotalCostwithBadDebtProvision = 22m,
                    EnglandWithBadDebtProvision = 8m,
                    WalesWithBadDebtProvision = 7m,
                    ScotlandWithBadDebtProvision = 6m,
                    NorthernIrelandWithBadDebtProvision = 5m,
                }
            };

            // Act
            var result = CalcResultCommsCostByMaterial2AJson.From(comms, materials);

             // Assert
            Assert.IsNotNull(result);

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);

            var expectedJson = """
                {
                "materialBreakdown": [
                  {
                    "materialName": "Aluminium",
                    "householdPackagingWasteTonnage": 100.25,
                    "publicBinTonnage": 50.50,
                    "totalTonnage": 150.75,
                    "pricePerTonne": "£0.4200",
                    "producerTotalCostWithoutBadDebtProvision": "£10.00",
                    "badDebtProvision": "£1.00",
                    "producerTotalCostWithBadDebtProvision": "£11.00",
                    "englandWithBadDebtProvision": "£4.00",
                    "walesWithBadDebtProvision": "£3.00",
                    "scotlandWithBadDebtProvision": "£2.00",
                    "northernIrelandWithBadDebtProvision": "£1.00"
                  },
                  {
                    "materialName": "Glass",
                    "householdPackagingWasteTonnage": 200,
                    "publicBinTonnage": 80,
                    "totalTonnage": 280,
                    "householdDrinksContainersTonnageGlass": 12.34,
                    "pricePerTonne": "£0.3000",
                    "producerTotalCostWithoutBadDebtProvision": "£20.00",
                    "badDebtProvision": "£2.00",
                    "producerTotalCostWithBadDebtProvision": "£22.00",
                    "englandWithBadDebtProvision": "£8.00",
                    "walesWithBadDebtProvision": "£7.00",
                    "scotlandWithBadDebtProvision": "£6.00",
                    "northernIrelandWithBadDebtProvision": "£5.00"
                  }
                ]
                }
                """;

            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
