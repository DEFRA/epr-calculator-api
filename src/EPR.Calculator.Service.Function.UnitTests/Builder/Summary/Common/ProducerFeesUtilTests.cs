using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class ProducerFeesUtilTests
{
    private readonly CalcResult calcResult;

    public ProducerFeesUtilTests()
    {
        calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                    ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
                },
                CalcResultPartialObligations = new CalcResultPartialObligations(){
                    PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
                },
            CalcResultParameterOtherCost       = TestDataHelper.GetCalcResultParameterOtherCost(),
            CalcResultDetail                   = TestDataHelper.GetCalcResultDetail(),
            CalcResultLaDisposalCostData       = TestDataHelper.GetCalcResultLaDisposalCostData(),
            CalcResultLapcapData               = TestDataHelper.GetCalcResultLapcapData(),
            CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
            ProducerFees                       = TestDataHelper.GetProducerFees(),
            CalcResultCommsCostReportDetail    = TestDataHelper.GetCalcResultCommsCostReportDetail(),
            CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            CalcResultProjectedProducers       = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };
    }

    private Fixture Fixture { get; } = new();

    public static ILookup<(int, string?), ProducerMaterialPackaging> ProjectedMaterialsLookup(List<ProducerDetail> producers)
    {
        // This allows us to retrofit into existing test setup, but ProducerReportedMaterials normally
        // refers to pre-processed data, which is _not_ what we want to display in the ResultsSummary
        ProducerMaterialPackaging ToProjected(ProducerReportedMaterial rm) =>
            new()
            {
                MaterialId                   = rm.MaterialId,
                ProducerDetailId             = rm.ProducerDetailId,
                PackagingType                = rm.PackagingType,
                PackagingTonnage             = rm.PackagingTonnage,
                PackagingTonnageRed          = rm.PackagingTonnageRed,
                PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical,
                SubmissionPeriod             = rm.SubmissionPeriod
            };

        return producers
            .SelectMany(p => p.ProducerReportedMaterials.Select(rm => (Key: (p.ProducerId, p.SubsidiaryId), Rm: ToProjected(rm))))
            .ToLookup(x => x.Key, x => x.Rm);
    }

    [TestMethod]
    public void CanGetHouseholdPackagingWasteTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.Household);

        // Assert
        Assert.AreEqual(1000.00m, result);
    }

    [TestMethod]
    public void CanGetPublicBinTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetHouseholdDrinksContainersTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetReportedTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = ProducerFeesUtil.GetReportedTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material);

        // Assert
        Assert.AreEqual(1000.00m, result);
    }

    [TestMethod]
    public void CanGetManagedConsumerWasteTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.ConsumerWaste);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetPricePerTonne_NonMatchingMaterial()
    {
        // Arrange
        var material = Fixture.Create<MaterialDetail>();

        // Act
        var result = ProducerFeesUtil.GetPricePerTonne(material, calcResult);

        // Assert
        Assert.AreEqual(new RamTonnageGroup{ Total = null, Red = null, Amber = null, Green = null }, result);
    }

    [TestMethod]
    public void CanGetPricePerTonne()
    {
        // Arrange
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = ProducerFeesUtil.GetPricePerTonne(material, calcResult);

        // Assert
        Assert.AreEqual(new RamTonnageGroup{ Total = 0.5889m, Red = null, Amber = null, Green= null }, result);
    }

    [TestMethod]
    public void CanGetProducerDisposalFee()
    {
        // Arrange
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        // Act
        var result = ProducerFeesUtil.GetProducerDisposalFee(material, calcResult, SelfManagedConsumerWasteData.Zero);

        // Assert
        Assert.AreEqual(new RamTonnageGroup{ Total = 0m, Red = null, Amber = null, Green = null }, result);
    }

    [TestMethod]
    public void CanGetProducerDisposalFee_WithModulation()
    {
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "AL");

        calcResult.CalcResultModulation = new ModulationResult
        {
            CalculatorRunId = 1,
            GreenFactor = 2,
            RedFactor = 4,
            ModulationByMaterial = new Dictionary<MaterialDetail, ModulationDetail>
            {
                [material] = mkModulationDetail(100, 120, 77.1423m, 90, 220, 550, 22000, 55000)
            }
        };

        var smcw = new SelfManagedConsumerWasteData
        {
            SmcwTonnage = 0,
            ActionedSmcwTonnage = new RamTonnageGroup { Total = 0m, Red = 0m, Amber = 0m, Green = 0m },
            ResidualSmcwTonnage = 0,
            NetTonnage = new RamTonnageGroup { Total = null, Red = 1m, Amber = 2m, Green = 3m }
        };

        var result = ProducerFeesUtil.GetProducerDisposalFee(material, calcResult, smcw);

        Assert.AreEqual(new RamTonnageGroup{ Total = 551.4269m, Red = 120, Amber = 200, Green = 231.4269m }, result);
    }


    [TestMethod]
    public void GetBadDebtProvision_ValidPercentage_WithPercent()
    {
        var result = ProducerFeesUtil.GetBadDebtProvision(calcResult, 200m);
        Assert.AreEqual(12m, result);
    }

    [TestMethod]
    public void GetProducerDisposalFeeWithBadDebtProvision_AddsPercentage()
    {
        var result = ProducerFeesUtil.GetProducerDisposalFeeWithBadDebtProvision(calcResult, 100m);
        Assert.AreEqual(106m, MathUtils.RoundAwayFromZero(result.Total, 10));
    }

    [TestMethod]
    public void CanGetCommsCostHeaderWithoutBadDebtFor2bTitle()
    {
        // Act
        var result = ProducerFeesUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);

        // Assert
        Assert.AreEqual(2531, result);
    }

    [TestMethod]
    public void CanGetReportedPublicBinTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 2);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "PL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.PublicBin);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    [TestMethod]
    public void CanGetReportedHDCTonnage()
    {
        // Arrange
        var producer = TestDataHelper.GetProducers().First(p => p.Id == 1);
        var material = TestDataHelper.GetMaterialDetails().First(m => m.Code == "GL");

        // Act
        var result = ProducerFeesUtil.GetTonnage(ProjectedMaterialsLookup(new List<ProducerDetail> { producer }), producer, material, PackagingTypes.HouseholdDrinksContainers);

        // Assert
        Assert.AreEqual(20.00m, result);
    }

    private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage() => Fixture.Create<CalcResultLateReportingTonnage>();

    private ModulationDetail mkModulationDetail(decimal adc, decimal rdc, decimal gdc, decimal at, decimal rt, decimal gt, decimal rAtAdc, decimal gAtAdc)
    {
        return new ModulationDetail
        {
            AmberMaterialDisposalCost             = adc,
            RedMaterialDisposalCost               = rdc,
            GreenMaterialDisposalCost             = gdc,
            AmberMaterialTonnages                 = at,
            RedMaterialTonnages                   = rt,
            GreenMaterialTonnages                 = gt,
            TotalRedMaterialAtAmberDisposalCost   = rAtAdc,
            TotalGreenMaterialAtAmberDisposalCost = gAtAdc
        };
    }
}
