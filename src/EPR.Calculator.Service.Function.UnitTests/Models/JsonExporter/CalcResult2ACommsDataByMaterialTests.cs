using System.Text.Json;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter;

[TestClass]
public class CalcResult2ACommsDataByMaterialTests
{
    [TestMethod]
    public void From_MapsMaterialsAndTotal()
    {
        // Arrange
        var comms = new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>
        {
            ["AL"] = new()
            {
                Cost = ByCountryCost.Empty with { England = 12.7305m },
                TotalCost = 12.7305m,
                HouseholdPackagingWasteTonnage = 2.34m,
                PublicBinTonnage = 4.56m,
                HouseholdDrinksContainersTonnage = 0m,
                LateReportingTonnage = 3.45m
            },
            ["GL"] = new()
            {
                Cost = ByCountryCost.Empty with { England = 32.0112m },
                TotalCost = 32.0112m,
                HouseholdPackagingWasteTonnage = 3.45m,
                PublicBinTonnage = 5.67m,
                HouseholdDrinksContainersTonnage = 0m,
                LateReportingTonnage = 4.56m
            }
        };
        var total = new CalcResultCommsCostCommsCostByMaterial
        {
            Cost = ByCountryCost.Empty,
            TotalCost = 0m,
            HouseholdPackagingWasteTonnage = 5.79m,
            PublicBinTonnage = 10.23m,
            HouseholdDrinksContainersTonnage = 0m,
            LateReportingTonnage = 8.01m
        };
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var result = CalcResult2ACommsDataByMaterial.From(materials, comms, total);

        // Assert
        Assert.IsNotNull(result);
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
        var expectedJson = """
            {
            "name": "2a Comms Costs - by Material",
            "calcResult2aCommsDataDetails": [
              {
                "materialName"                          : "Aluminium",
                "englandCommsCost"                      : "£12.73",
                "walesCommsCost"                        : "£0.00",
                "scotlandCommsCost"                     : "£0.00",
                "northernIrelandCommsCost"              : "£0.00",
                "totalCommsCost"                        : "£12.73",
                "commsCostByMaterialPricePerTonne"      : "£1.2300",
                "producerHouseholdPackagingWasteTonnage": 2.340,
                "publicBinTonnage"                      : 4.560,
                "householdDrinksContainersTonnage"      : 0,
                "lateReportingTonnage"                  : 3.450,
                "totalTonnage"                          : 10.350
              },
              {
                "materialName"                          : "Glass",
                "englandCommsCost"                      : "£32.01",
                "walesCommsCost"                        : "£0.00",
                "scotlandCommsCost"                     : "£0.00",
                "northernIrelandCommsCost"              : "£0.00",
                "totalCommsCost"                        : "£32.01",
                "commsCostByMaterialPricePerTonne"      : "£2.3400",
                "producerHouseholdPackagingWasteTonnage": 3.450,
                "publicBinTonnage"                      : 5.670,
                "householdDrinksContainersTonnage"      : 0,
                "lateReportingTonnage"                  : 4.560,
                "totalTonnage"                          : 13.680
              }
            ],
            "calcResult2aCommsDataDetailsTotal": {
              "total"                                      : "Total",
              "englandCommsCostTotal"                      : "£0.00",
              "walesCommsCostTotal"                        : "£0.00",
              "scotlandCommsCostTotal"                     : "£0.00",
              "northernIrelandCommsCostTotal"              : "£0.00",
              "totalCommsCostTotal"                        : "£0.00",
              "producerHouseholdPackagingWasteTonnageTotal": 5.79,
              "publicBinTonnage"                           : 10.23,
              "householdDrinksContainersTonnageTotal"      : 0,
              "lateReportingTonnageTotal"                  : 8.01,
              "totalTonnageTotal"                          : 24.03
            }
            }
            """;
        JsonTestUtils.AssertJson(expectedJson, json);
    }
}
