using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.SaSetupCosts;

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
            ByMaterial = []
        },
        CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
        CalcResultSummary = new CalcResultSummary
        {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                    ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                    ProducerId = "1",
                    ProducerName = "Test",
                    TotalProducerDisposalFeeWithBadDebtProvision = 100,
                    TotalProducerCommsFeeWithBadDebtProvision = 100,
                    SubsidiaryId = "1",
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1
                }
            }
        },
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = TestDataHelper.GetCalcResultLateReportingTonnage(),
        CalcResultProjectedProducers = new CalcResultProjectedProducers(){
            H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
            H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
        }
    };

    [TestMethod]
    public void CanCallSaSetupCostsProducerFeeWithoutBadDebtProvision()
    {
        // Act
        SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, calcResult.CalcResultSummary);

        // Assert
        Assert.AreEqual(100, calcResult.CalcResultSummary.SaSetupCostsTitleSection5);
        Assert.AreEqual(6, calcResult.CalcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5);
        Assert.AreEqual(106, calcResult.CalcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5);
        Assert.AreEqual(1, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision);
        Assert.AreEqual(0.06m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.BadDebtProvision);
        Assert.AreEqual(1.06m, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithBadDebtProvision);
    }

    [TestMethod]
    public void CanCallGetSaSetupCostsEnglandOverallTotalWithBadDebtProvision()
    {
        SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, calcResult.CalcResultSummary);
        // Act
        var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(calcResult, calcResult.CalcResultSummary.SaSetupCostsTitleSection5, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England);

        // Assert
        Assert.AreEqual(0.42m, Math.Round(result, 2));
    }

    [TestMethod]
    public void CanCallGetSaSetupCostsScotlandOverallTotalWithBadDebtProvision()
    {
        SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, calcResult.CalcResultSummary);

        // Act
        var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(calcResult, calcResult.CalcResultSummary.SaSetupCostsTitleSection5, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland);

        // Assert
        Assert.AreEqual(0.16m, Math.Round(result, 2));
    }

    [TestMethod]
    public void CanCallGetSaSetupCostsWalesOverallTotalWithBadDebtProvision()
    {
        SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, calcResult.CalcResultSummary);

        // Act
        var result = SaSetupCostsProducer.GetCountryTotalWithBadDebtProvision(calcResult, calcResult.CalcResultSummary.SaSetupCostsTitleSection5, calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales);

        // Assert
        Assert.AreEqual(0.11m, Math.Round(result, 2));
    }
}
