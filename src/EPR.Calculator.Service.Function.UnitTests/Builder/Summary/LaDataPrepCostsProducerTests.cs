using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class LaDataPrepCostsProducerTests
{
    private readonly CalcResult calcResult = new()
    {
        CalcResultScaledupProducers = new CalcResultScaledupProducers(){
            ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
        },
        CalcResultPartialObligations = new CalcResultPartialObligations(){
            PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
        },
        CalcResultParameterOtherCost = new CalcResultParameterOtherCost
        {
            BadDebtValue = 6m,
            LaDataPrepCharge = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            CountryApportionment = new ByCountryApportionment { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
            SaOperatingCost = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 },
            SchemeSetupCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
        CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData { ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>() },
        CalcResultLapcapData = new CalcResultLapcapData { ByMaterial = new Dictionary<string, ByCountryCost>() },
        CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost = new ByCountryCost { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M },
            LADataPrepCharge = new ByCountryCost { England = 0.10M, Wales = 20M, Scotland = 0.15M, NorthernIreland = 0.15M }
        },
        ProducerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail>
            {
                new()
                {
                    FeeDetail = new FeeDetail
                    {
                        FeesByMaterial =
                            new Dictionary<string, Fees>(),
                        ProducerId = 1,
                        SubsidiaryId = "1",
                        ProducerName = "Test",
                        CommsCostsSection2c = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 10, Wales = 0, Scotland = 0, NorthernIreland = 0 } },
                        TotalOnePlus2A2B2CWithBadDebtPercentage = 100,
                        LaDataPrepSection4 = new FeeWithBadDebt
                        {
                            FeeWithoutBadDebt = 100,
                            BadDebt           = 20,
                            ByCountry    = new ByCountryCost { England = 20, Wales = 20, Scotland = 20, NorthernIreland = 20 }
                        },
                        BillingInstruction = new BillingInstruction
                        {
                            SuggestedBillingInstruction = string.Empty
                        }
                    }
                }
            },
            Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty, TotalOnePlus2A2B2CWithBadDebtPercentage = 100 }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    [TestMethod]
    public void LaDataPrepCostsProducer_CanCallSetValues()
    {
        // Act
        LaDataPrepCostsProducer.SetValues(calcResult, calcResult.ProducerFees);

        // Assert
        Assert.AreEqual(100   , calcResult.ProducerFees.Total.LaDataPrepSection4.FeeWithoutBadDebt);
        Assert.AreEqual(6     , calcResult.ProducerFees.Total.LaDataPrepSection4.BadDebt);
        Assert.AreEqual(106   , calcResult.ProducerFees.Total.LaDataPrepSection4.ByCountry.Total);
        Assert.AreEqual(100   , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.FeeWithoutBadDebt);
        Assert.AreEqual(6     , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.BadDebt);
        Assert.AreEqual(106   , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.ByCountry.Total);
        Assert.AreEqual(42.40m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.ByCountry.England);
        Assert.AreEqual(31.80m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.ByCountry.Wales);
        Assert.AreEqual(21.20m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.ByCountry.Scotland);
        Assert.AreEqual(10.60m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.LaDataPrepSection4!.ByCountry.NorthernIreland);
    }
}
