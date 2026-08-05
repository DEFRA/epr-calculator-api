using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.JsonExporter.Model;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.Service.Function.UnitTests.Utils;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.JsonExporter.Model;

using static JsonNodeComparer;

[TestClass]
public class CalculationResultsJsonFromTests
{
    [TestMethod]
    public void From_ValuesAreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!["producerCalculationResultsSummary"];

        // Assert
        Assert.IsNotNull(roundTrippedData);

        // 1
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.FeeWithoutBadDebt),
            roundTrippedData["feeForLaDisposalCostsWithoutBadDebtprovision1"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.BadDebt),
            roundTrippedData["badDebtProvision1"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LADisposalCostsSection1.ByCountry.Total),
            roundTrippedData["feeForLaDisposalCostsWithBadDebtprovision1"]);

        // 2a
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.FeeWithoutBadDebt),
            roundTrippedData["feeForCommsCostsByMaterialWithoutBadDebtprovision2a"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.BadDebt),
            roundTrippedData["badDebtProvision2a"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2a.ByCountry.Total),
            roundTrippedData["feeForCommsCostsByMaterialWitBadDebtprovision2a"]);

        // 2b
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.FeeWithoutBadDebt),
            roundTrippedData["feeForCommsCostsUkWideWithoutBadDebtprovision2b"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.BadDebt),
            roundTrippedData["badDebtProvision2b"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2b.ByCountry.Total),
            roundTrippedData["feeForCommsCostsUkWideWithBadDebtprovision2b"]);

        // 2c
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.FeeWithoutBadDebt),
            roundTrippedData["feeForCommsCostsByCountryWithoutBadDebtprovision2c"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.BadDebt),
            roundTrippedData["badDebtProvision2c"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.CommsCostsSection2c.ByCountry.Total),
            roundTrippedData["feeForCommsCostsByCountryWideWithBadDebtprovision2c"]);

        // 1+2a+2b+2c
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.TotalOnePlus2A2B2CWithBadDebt()),
            roundTrippedData["total12a2b2cWithBadDebt"]);

        // 3
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.FeeWithoutBadDebt),
            roundTrippedData["saOperatingCostsWithoutBadDebtProvision3"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.BadDebt),
            roundTrippedData["badDebtProvision3"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaOperatingCostsSection3.ByCountry.Total),
            roundTrippedData["saOperatingCostsWithBadDebtProvision3"]);

        // 4
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.FeeWithoutBadDebt),
            roundTrippedData["laDataPrepCostsWithoutBadDebtProvision4"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.BadDebt),
            roundTrippedData["badDebtProvision4"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.LaDataPrepSection4.ByCountry.Total),
            roundTrippedData["laDataPrepCostsWithbadDebtProvision4"]);

        // 5
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.FeeWithoutBadDebt),
            roundTrippedData["oneOffFeeSaSetupCostsWithoutBadDebtProvision5"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.BadDebt),
            roundTrippedData["badDebtProvision5"]);
        AssertAreEqual(FormatUtils.FormatCurrency(data.Total.SaSetupCostsSection5.ByCountry.Total),
            roundTrippedData["oneOffFeeSaSetupCostsWithBadDebtProvision5"]);
    }

    [TestMethod]
    public void From_ProducerDisposalFeesWithBadDebtProvision1_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);

        var actual = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"]!["materialBreakdown"]![0]!;
        var producer = calcResult.ProducerFees.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;
        var expected = producer.FeeDetail.DisposalFeesByMaterial.First();

        decimal? actualValue = 0;
        if(actual["previousInvoicedTonnage"]?.ToString() == "-")
        {
            actualValue = null;
        }

        Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actualValue);
        Assert.AreEqual(expected.Value.HhTonnage.TotalRamTonnage(), actual["householdPackagingWasteTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.PbTonnage.TotalRamTonnage(), actual["publicBinTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.TotalTonnage.TotalRamTonnage(), actual["totalTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.SmcwTonnage, actual["selfManagedConsumerWasteTonnage"]!.GetValue<decimal>());
        Assert.AreEqual(expected.Value.NetTonnage.Total, actual["netTonnage"]!.GetValue<decimal>());

        var actualPrev = ReadNullableDecimal(actual, "previousInvoicedTonnage");
        Assert.AreEqual(expected.Value.PreviousInvoicedTonnage, actualPrev);

        var actualChange = ReadNullableDecimal(actual, "tonnageChange");
        Assert.AreEqual(expected.Value.TonnageChange, actualChange);

        Assert.AreEqual("£0.6676", actual["pricePerTonne"]!.GetValue<string>());
        Assert.AreEqual("£607.53", actual["producerDisposalFeeWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£36.45" , actual["badDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£643.98", actual["producerDisposalFeeWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£348.06", actual["englandWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£78.46" , actual["walesWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£156.28", actual["scotlandWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual("£61.18" , actual["northernIrelandWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var actual = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"]!;
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;

        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.TotalOnePlus2A2B2CWithBadDebt()), actual["totalFeeWithBadDebtProvision"]);
        AssertAreEqual($"{producer.FeeDetail.TotalOnePlus2A2B2CWithBadDebtPercentage.ToString("F8")}%", actual["producerPercentageOfOverallProducerCost"]);
    }

    [TestMethod]
    public void From_CommsCost2AValues_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoACosts = roundTrippedData[0]?["commsCostsByMaterialFeesSummary2a"];
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.FeeWithoutBadDebt, twoACosts?["totalProducerFeeForCommsCostsWithoutBadDebtProvision2a"]!);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.BadDebt, twoACosts?["totalBadDebtProvision"]);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.ByCountry.Total, twoACosts?["totalProducerFeeForCommsCostsWithBadDebtProvision2a"]!);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.ByCountry.England, twoACosts?["englandTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.ByCountry.Wales, twoACosts?["walesTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.ByCountry.Scotland, twoACosts?["scotlandTotalWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.CommsCostsSection2a.ByCountry.NorthernIreland, twoACosts?["northernIrelandTotalWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_FeeForSASetUpCostsWithBadDebtProvision_5_ReturnsValidValues()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var actual = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"]!;
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;

        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5!.FeeWithoutBadDebt), actual["totalProducerOneOffFeeForSASetUpCostsWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.BadDebt), actual["badDebtProvisionFor5"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.ByCountry.Total), actual["totalProducerOneOffFeeForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.ByCountry.England), actual["englandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.ByCountry.Wales), actual["walesTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.ByCountry.Scotland), actual["scotlandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.SaSetupCostsSection5.ByCountry.NorthernIreland), actual["northernIrelandTotalForSASetUpCostsWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCost3SA_Operating_Costs_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var threeSACosts = roundTrippedData[0]!["feeForSAOperatingCostsWithBadDebtProvision_3"];
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(threeSACosts);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3!.FeeWithoutBadDebt            , threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithoutBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.BadDebt                       , threeSACosts["badDebProvisionFor3"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.ByCountry.Total          , threeSACosts["totalProducerFeeForSAOperatingCosts_1_2a_2b_2c_WithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.ByCountry.England        , threeSACosts["englandTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.ByCountry.Wales          , threeSACosts["walesTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.ByCountry.Scotland       , threeSACosts["scotlandTotalForSAOperatingCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.SaOperatingCostsSection3.ByCountry.NorthernIreland, threeSACosts["northernIrelandTotalForSAOperatingCostsWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_FeeForCommsCostsWithBadDebtProvision2a_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoACosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2a"];
        Assert.IsNotNull(twoACosts);
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.FeeWithoutBadDebt), twoACosts["totalProducerFeeForCommsCostsWithoutBadDebtProvision"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.BadDebt), twoACosts["badDebtProvisionFor2a"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.ByCountry.Total), twoACosts["totalProducerFeeForCommsCostsWithBadDebtProvision"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.ByCountry.England), twoACosts["englandTotalWithBadDebtProvision"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.ByCountry.Wales), twoACosts["walesTotalWithBadDebtProvision"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.ByCountry.Scotland), twoACosts["scotlandTotalWithBadDebtProvision"]);
        AssertAreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2a.ByCountry.NorthernIreland), twoACosts["northernIrelandTotalWithBadDebtProvision"]);
    }

    [TestMethod]
    public void From_FeeForCommsCostsWithBadDebtProvision2b_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoBCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2b"];
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(twoBCosts);
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b!.FeeWithoutBadDebt            ), twoBCosts["totalProducerFeeForCommsCostsUKWideWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.BadDebt                       ), twoBCosts["badDebtProvisionFor2b"                                     ]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.ByCountry.Total          ), twoBCosts["totalProducerFeeForCommsCostsUKWideWithBadDebtProvision"   ]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.ByCountry.England        ), twoBCosts["englandTotalWithBadDebtProvision"                          ]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.ByCountry.Wales          ), twoBCosts["walesTotalWithBadDebtProvision"                            ]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.ByCountry.Scotland       ), twoBCosts["scotlandTotalWithBadDebtProvision"                         ]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2b.ByCountry.NorthernIreland), twoBCosts["northernIrelandTotalWithBadDebtProvision"                  ]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCost2CValues_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var twoCCosts = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2c"];
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        Assert.IsNotNull(twoCCosts);
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.FeeWithoutBadDebt), twoCCosts["totalProducerFeeForCommsCostsByCountryWithoutBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.BadDebt), twoCCosts["badDebProvisionFor2c"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.ByCountry.Total), twoCCosts["totalProducerFeeForCommsCostsByCountryWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.ByCountry.England), twoCCosts["englandTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.ByCountry.Wales), twoCCosts["walesTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.ByCountry.Scotland), twoCCosts["scotlandTotalWithBadDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.CommsCostsSection2c.ByCountry.NorthernIreland), twoCCosts["northernIrelandTotalWithBadDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_DisposalFeeSummary1()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        var disposalFeeSummary1 = roundTrippedData[0]!["disposalFeeSummary1"]!;
        Assert.IsNotNull(roundTrippedData);
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;

        // Disposal Fee
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.FeeWithoutBadDebt,
            disposalFeeSummary1["totalProducerDisposalFeeWithoutBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.BadDebt,
            disposalFeeSummary1["badDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Total,
            disposalFeeSummary1["totalProducerDisposalFeeWithBadDebtProvision"]!);

        // Countries
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.England,
            disposalFeeSummary1["englandTotal"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Wales,
            disposalFeeSummary1["walesTotal"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Scotland,
            disposalFeeSummary1["scotlandTotal"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.NorthernIreland,
            disposalFeeSummary1["northernIrelandTotal"]!);

        // Tonnage Change
        Assert.AreEqual(producer.FeeDetail.TonnageChangeCount,
            disposalFeeSummary1["tonnageChangeCount"]?.ToString());
        Assert.AreEqual(producer.FeeDetail.TonnageChangeAdvice,
            disposalFeeSummary1["tonnageChangeAdvice"]?.ToString());
    }

    [TestMethod]
    public void From_BillingInstructions_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(obj, options);

        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);
        Assert.IsNotNull(roundTrippedData[0]);
        var billingInstructions = roundTrippedData[0]?["calculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
        Assert.IsNotNull(billingInstructions);
    }

    [TestMethod]
    public void From_FeeForLADisposalCost1_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var node = JsonNode.Parse(json);
        var roundTrippedData = node?["producerCalculationResults"]?.AsArray();

        // Assert
        Assert.IsNotNull(roundTrippedData);

        var feeForLADisposalCosts1 = roundTrippedData[0]?["feeForLADisposalCosts1"];
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        if (producer == null)
        {
            Assert.Fail("Producer not found.");
        }
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1!.FeeWithoutBadDebt, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithoutBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.BadDebt, feeForLADisposalCosts1?["badDebtProvisionForLADisposalCosts"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Total, feeForLADisposalCosts1?["totalProducerFeeForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.England, feeForLADisposalCosts1?["englandTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Wales, feeForLADisposalCosts1?["walesTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.Scotland, feeForLADisposalCosts1?["scotlandTotalForLADisposalCostsWithBadDebtProvision"]!);
        AssertAreEqual(producer.FeeDetail.LADisposalCostsSection1.ByCountry.NorthernIreland, feeForLADisposalCosts1?["northernIrelandTotalForLADisposalCostsWithBadDebtProvision"]!);
    }

    [TestMethod]
    public void From_ProducerIdSubsidiaryId_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];

        // Assert
        Assert.IsNotNull(roundTrippedData);

        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        if (producer == null)
        {
            Assert.Fail("Producer not found.");
        }
        Assert.AreEqual(producer.FeeDetail.ProducerId.ToString(), roundTrippedData[0]!["producerID"]?.ToString());
        Assert.AreEqual(producer.FeeDetail.SubsidiaryId, roundTrippedData[0]!["subsidiaryID"]?.ToString());
        Assert.AreEqual(producer.FeeDetail.ProducerName, roundTrippedData[0]!["producerName"]?.ToString());
        Assert.AreEqual(producer.FeeDetail.TradingName, roundTrippedData[0]!["tradingName"]?.ToString());
        Assert.AreEqual(producer.FeeDetail.Level ?? "1" , roundTrippedData[0]!["level"]?.ToString());
        var expectedScaledup = calcResult.CalcResultScaledupProducers.ScaledupProducers.Exists(p => p.ProducerId == producer.FeeDetail.ProducerId) ? CommonConstants.Yes : CommonConstants.No;
        Assert.AreEqual(expectedScaledup, roundTrippedData[0]!["scaledUpTonnages"]?.ToString());
    }

    [TestMethod]
    public void From_ProducerCalculationResultsTotal_CanBeNull()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var producerCalculationResultsTotal = JsonSerializer.Deserialize<JsonObject>(json)!["producerCalculationResultsTotal"]!;

        // Assert
        Assert.IsNull(producerCalculationResultsTotal);
    }

    [TestMethod]
    public void From_FeeForLADataPrepCostsWithBadDebtProvision_4_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
            ["producerCalculationResults"];


        // Assert
        Assert.IsNotNull(roundTrippedData);
        Assert.IsNotNull(roundTrippedData[0]);
        var costs = roundTrippedData[0]!["feeForLADataPrepCostsWithBadDebtProvision_4"];
        Assert.IsNotNull(costs);

        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level));
        Assert.IsNotNull(producer);
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4!.FeeWithoutBadDebt), costs["totalProducerFeeForLADataPrepCostsWithoutBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.BadDebt), costs["badDebtProvisionFor4"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.ByCountry.Total), costs["totalProducerFeeForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.ByCountry.England), costs["englandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.ByCountry.Wales), costs["walesTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.ByCountry.Scotland), costs["scotlandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
        Assert.AreEqual(FormatUtils.FormatCurrency(producer.FeeDetail.LaDataPrepSection4.ByCountry.NorthernIreland), costs["northernIrelandTotalForLADataPrepCostsWithBadDebtProvision"]!.GetValue<String>());
    }

    [TestMethod]
    public void From_CalculationResultsJson_AreValid()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)!
                ["producerCalculationResults"]!;

        // Assert
        var calculationResult = roundTrippedData[0]!;
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;

        // Main Fields
        Assert.AreEqual(producer.FeeDetail.ProducerId.ToString(), calculationResult["producerID"]?.GetValue<string>());
        Assert.AreEqual(producer.FeeDetail.SubsidiaryId, calculationResult["subsidiaryID"]?.GetValue<string>());
        Assert.AreEqual(producer.FeeDetail.ProducerName, calculationResult["producerName"]?.GetValue<string>());
        Assert.AreEqual(producer.FeeDetail.TradingName!, calculationResult["tradingName"]?.GetValue<string>());
        Assert.AreEqual(int.Parse(producer.FeeDetail.Level!), calculationResult["level"]?.GetValue<int>());
        var expectedScaledup2 = calcResult.CalcResultScaledupProducers.ScaledupProducers.Exists(p => p.ProducerId == producer.FeeDetail.ProducerId) ? CommonConstants.Yes : CommonConstants.No;
        Assert.AreEqual(expectedScaledup2, calculationResult["scaledUpTonnages"]?.GetValue<string>());

        // Sub-Sections
        var producerDisposalFeesWithBadDebtProvision1 = roundTrippedData[0]!["producerDisposalFeesWithBadDebtProvision1"];
        Assert.IsNotNull(producerDisposalFeesWithBadDebtProvision1);
        var disposalFeeSummary1 = roundTrippedData[0]!["disposalFeeSummary1"];
        Assert.IsNotNull(disposalFeeSummary1);
        var feesForCommsCostsWithBadDebtProvision2a = roundTrippedData[0]!["feesForCommsCostsWithBadDebtProvision2a"];
        Assert.IsNotNull(feesForCommsCostsWithBadDebtProvision2a);
        var commsCostsByMaterialFeesSummary2a = roundTrippedData[0]!["commsCostsByMaterialFeesSummary2a"];
        Assert.IsNotNull(commsCostsByMaterialFeesSummary2a);
        var feeForLADisposalCosts1 = roundTrippedData[0]!["feeForLADisposalCosts1"];
        Assert.IsNotNull(feeForLADisposalCosts1);
        var feeForCommsCostsWithBadDebtProvision_2a = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2a"];
        Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2a);
        var feeForCommsCostsWithBadDebtProvision_2b = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2b"];
        Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2b);
        var feeForCommsCostsWithBadDebtProvision_2c = roundTrippedData[0]!["feeForCommsCostsWithBadDebtProvision_2c"];
        Assert.IsNotNull(feeForCommsCostsWithBadDebtProvision_2c);
        var totalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c = roundTrippedData[0]!["totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c"];
        Assert.IsNotNull(totalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c);
        var feeForSAOperatingCostsWithBadDebtProvision_3 = roundTrippedData[0]!["feeForSAOperatingCostsWithBadDebtProvision_3"];
        Assert.IsNotNull(feeForSAOperatingCostsWithBadDebtProvision_3);
        var feeForLADataPrepCostsWithBadDebtProvision_4 = roundTrippedData[0]!["feeForLADataPrepCostsWithBadDebtProvision_4"];
        Assert.IsNotNull(feeForLADataPrepCostsWithBadDebtProvision_4);
        var feeForSASetUpCostsWithBadDebtProvision_5 = roundTrippedData[0]!["feeForSASetUpCostsWithBadDebtProvision_5"];
        Assert.IsNotNull(feeForSASetUpCostsWithBadDebtProvision_5);
        var totalProducerBillWithBadDebtProvision = roundTrippedData[0]!["totalProducerBillWithBadDebtProvision"];
        Assert.IsNotNull(totalProducerBillWithBadDebtProvision);
        var calculationOfSuggestedBillingInstructionsAndInvoiceAmounts = roundTrippedData[0]!["calculationOfSuggestedBillingInstructionsAndInvoiceAmounts"];
        Assert.IsNotNull(calculationOfSuggestedBillingInstructionsAndInvoiceAmounts);
    }

    [TestMethod]
    public void From_ProducerCalculationResult_Level1_AreDisplayed()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();
        var data = calcResult.ProducerFees;
        var materials = TestDataHelper.GetMaterialDetails();

        data.Details.First().FeeDetail.Level = "1";

        // Act
        var obj = CalculationResultsJson.From(TestDataHelper.BillingRun2025, calcResult, materials);
        var json = JsonSerializer.Serialize(obj);
        var roundTrippedData = JsonSerializer.Deserialize<JsonObject>(json)![
                "producerCalculationResults"]!;

        // Assert
        var calculationResult = roundTrippedData[0]!;
        var producer = data.Details.SingleOrDefault(t => !string.IsNullOrEmpty(t.FeeDetail.Level))!;

        // Main Fields
        AssertAreEqual(producer.FeeDetail.ProducerId.ToString(), calculationResult["producerID"]);
    }

    private static decimal? ReadNullableDecimal(JsonNode obj, string prop)
    {
        var n = obj[prop];
        if (n is null) return null;

        //May emit numbers or strings ("-" for null)
        var s = n.ToString();
        if (string.Equals(s, "-", StringComparison.Ordinal)) return null;

        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
            ? d
            : null;
    }

}
