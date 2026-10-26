using System.Text.Json;
using System.Text.Json.Nodes;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Exporter.JsonExporter.Model;

/// <summary>
/// Tests for the per-producer fields produced by BillingFileJson.From() in the "producers" array,
/// replacing the removed CalculationResultsJson class.
/// </summary>
[TestClass]
public class ProducerResultsTests
{
    private static JsonNode GetProducers()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        var materials  = TestDataHelper.GetMaterialDetails();
        var producers  = calcResult.ProducerFees.Details
            .Select(p => ProducerResult.From(p.FeeDetail, materials, applyModulation: false))
            .ToList();
        return JsonNode.Parse(JsonSerializer.Serialize(producers))!;
    }

    private static FeeDetail GetFirstProducer()
    {
        var calcResult = TestDataHelper.GetCalcResult();
        return calcResult.ProducerFees.Details.First().FeeDetail;
    }

    private static string F2(decimal value) =>
        value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

    private static string F4(decimal value) =>
        value.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

    [TestMethod]
    public void From_ProducerIdentity_IsValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();

        Assert.IsNotNull(producers[0]);
        Assert.AreEqual(producer.ProducerId.ToString(), producers[0]!["producerID"]?.ToString());
        Assert.AreEqual(producer.SubsidiaryId,          producers[0]!["subsidiaryID"]?.ToString());
        Assert.AreEqual(producer.ProducerName,          producers[0]!["producerName"]?.ToString());
    }

    [TestMethod]
    public void From_DisposalCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["disposalCosts"]!;
        var expected  = producer.LADisposalCostsSection1;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt),       actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),                  actual["badDebtProvision"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.ByCountry.Total),     actual["total"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.ByCountry.England),   actual["england"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.ByCountry.Wales),     actual["wales"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.ByCountry.Scotland),  actual["scotland"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.ByCountry.NorthernIreland), actual["northernIreland"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsByMaterial_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsByMaterial"]!;
        var expected  = producer.CommsCostsSection2a;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsUKWide_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsUKWide"]!;
        var expected  = producer.CommsCostsSection2b;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_CommsCostsByCountry_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["commsCostsByCountry"]!;
        var expected  = producer.CommsCostsSection2c;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_SaOperatingCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["saOperatingCosts"]!;
        var expected  = producer.SaOperatingCostsSection3;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_LaDataPrepCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["laDataPrepCosts"]!;
        var expected  = producer.LaDataPrepSection4;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_SaSetUpCosts_AreValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var actual    = producers[0]!["saSetUpCosts"]!;
        var expected  = producer.SaSetupCostsSection5;

        Assert.AreEqual(F2(expected.FeeWithoutBadDebt), actual["base"]!.GetValue<string>());
        Assert.AreEqual(F2(expected.BadDebt),            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_TotalBill_IsValid()
    {
        var producers = GetProducers();
        var producer  = GetFirstProducer();
        var expected  = producer.TotalBillBreakdown;

        Assert.AreEqual(F2(expected.ByCountry.Total), producers[0]!["totalBill"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_Invoice_IsPresent()
    {
        var producers = GetProducers();
        var invoice   = producers[0]!["invoice"];
        Assert.IsNotNull(invoice);
        Assert.IsNotNull(invoice!["instruction"]);
    }

    [TestMethod]
    public void From_DisposalFeesByMaterial_ContainsMaterialBreakdown()
    {
        var producers  = GetProducers();
        var producer   = GetFirstProducer();
        var byMaterial = producers[0]!["disposalFeesByMaterial"]!.AsArray();

        Assert.IsNotNull(byMaterial);
        Assert.IsTrue(byMaterial.Count > 0);

        var firstMaterial = producer.DisposalFeesByMaterial.First();
        var actual = byMaterial[0]!;

        Assert.IsNotNull(actual["material"]);
        Assert.IsNotNull(actual["feeWithoutBadDebt"]);
        Assert.IsNotNull(actual["fee"]);
        Assert.IsNotNull(actual["badDebtProvision"]);
        Assert.IsNotNull(actual["england"]);
        Assert.AreEqual(
            firstMaterial.Value.BadDebt.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            actual["badDebtProvision"]!.GetValue<string>());
    }

    [TestMethod]
    public void From_Materials_ContainsAllMaterials()
    {
        var calcResult    = TestDataHelper.GetCalcResult();
        var materials     = TestDataHelper.GetMaterialDetails();
        var materialsList = MaterialPrices.FromAll(materials, calcResult, applyModulation: false).ToList();

        Assert.AreEqual(materials.Count, materialsList.Count);
        Assert.IsTrue(materialsList.All(m => m.DisposalPricePerTonne != null));
        Assert.IsTrue(materialsList.All(m => m.CommsPricePerTonne != null));
    }

    [TestMethod]
    public void From_Materials_WithoutModulation_UsesLaDisposalPricePerTonne()
    {
        var calcResult    = TestDataHelper.GetCalcResult();
        var materials     = TestDataHelper.GetMaterialDetails();
        var materialsList = MaterialPrices.FromAll(materials, calcResult, applyModulation: false).ToList();

        foreach (var (material, prices) in materials.Zip(materialsList))
        {
            var expected = calcResult.CalcResultLaDisposalCostData.ByMaterial.GetValueOrDefault(material.Code)?.DisposalCostPricePerTonne ?? 0;
            Assert.IsNull(prices.DisposalPricePerTonne.Modulated);
            Assert.AreEqual(F4(expected), prices.DisposalPricePerTonne.Value);
        }
        Assert.IsTrue(materialsList.Any(m => m.DisposalPricePerTonne.Value != "0.0000"));
    }
}
