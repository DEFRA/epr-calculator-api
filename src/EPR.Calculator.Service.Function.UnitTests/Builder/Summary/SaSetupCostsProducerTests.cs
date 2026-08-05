using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class SaSetupCostsProducerTests
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
            SaOperatingCost = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 },
            SchemeSetupCost = new ByCountryCost { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 }
        },
        CalcResultDetail = new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) },
        CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                ["Material1"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 33m,
                        PublicBinTonnage = 66m,
                        HouseholdDrinkContainersTonnage = 0
                    },
                ["Material2"] =
                    new()
                    {
                        Cost = ByCountryCost.Empty,
                        HouseholdPackagingWasteTonnage = 133m,
                        PublicBinTonnage = 166m,
                        HouseholdDrinkContainersTonnage = 0
                    }
            }
        },
        CalcResultLapcapData = new CalcResultLapcapData
        {
            ByMaterial = new Dictionary<string, ByCountryCost>()
        },
        CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
        ProducerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail>
            {
                new()
                {
                    FeeDetail = new FeeDetail
                    {
                        FeesByMaterial = new Dictionary<string, Fees>(),
                        ProducerId = 1,
                        ProducerName = "Test",
                        SubsidiaryId = "1",
                        TotalOnePlus2A2B2CWithBadDebtPercentage = 1
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
    public void SaSetupCostsProducer_CanCallSetValues()
    {
        // Act
        SaSetupCostsProducer.SetValues(calcResult, calcResult.ProducerFees);

        // Assert
        Assert.AreEqual(100    , calcResult.ProducerFees.Total.SaSetupCostsSection5.FeeWithoutBadDebt);
        Assert.AreEqual(6      , calcResult.ProducerFees.Total.SaSetupCostsSection5.BadDebt);
        Assert.AreEqual(106    , calcResult.ProducerFees.Total.SaSetupCostsSection5.ByCountry.Total);
        Assert.AreEqual(1      , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.FeeWithoutBadDebt);
        Assert.AreEqual(0.06m  , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.BadDebt);
        Assert.AreEqual(1.06m  , calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.ByCountry.Total);
        Assert.AreEqual(0.4240m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.ByCountry.England);
        Assert.AreEqual(0.1060m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.ByCountry.Wales);
        Assert.AreEqual(0.1590m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.ByCountry.Scotland);
        Assert.AreEqual(0.3710m, calcResult.ProducerFees.Details.ToList()[0].FeeDetail.SaSetupCostsSection5!.ByCountry.NorthernIreland);
    }
}
