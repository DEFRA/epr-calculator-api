using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder.Summary.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultSummaryCommsCostTwoATests
{
    private readonly CalcResult calcResult;
    private readonly MaterialDetail material;
    private readonly List<ProducerDetail> producers;

    public CalcResultSummaryCommsCostTwoATests()
    {
        material = GetMaterial();
        producers = GetProducers();

            calcResult = new CalcResult
            {
                CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                    ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
                },
                CalcResultPartialObligations = new CalcResultPartialObligations(){
                    PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
                },
                CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
                CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
                CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
                CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
                CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
                ProducerFees = TestDataHelper.GetProducerFees(),
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLateReportingTonnageData = this.GetCalcResultLateReportingTonnage(),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                    H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                    H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
                },
            };
        }

    private Fixture Fixture { get; } = new();

    [TestMethod]
    public void GetPriceperTonneForComms_ShouldReturnCorrectTotal()
    {
        // Act
        var totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult);

        // Assert
        Assert.AreEqual(0.42m, totalCost);
    }

    [TestMethod]
    public void GetPriceperTonneForComms_WhenNoMaterialMatch_ShouldReturn0()
    {
        // Arrange
        var material2 = GetMaterial() with { Code = "Unknown" };

        // Act
        var totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material2, calcResult);

        // Assert
        Assert.AreEqual(0m, totalCost);
    }

    [TestMethod]
    public void GetCommsFeesCosts_ShouldReturnCorrectValues()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoA.GetCommsFeesCosts(ProducerFeesUtilTests.ProjectedMaterialsLookup(producers), producers[0], material, calcResult);

        // Assert
        Assert.AreEqual(504.00m, result.FeeWithoutBadDebt);
        Assert.AreEqual(30.24m, result.BadDebt);
        Assert.AreEqual(534.2400m, result.ByCountry.Total);
    }

    private static List<ProducerDetail> GetProducers()
    {
        var producers = TestDataHelper.GetProducers();

        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                MaterialId = 1,
                ProducerDetailId = 1,
                PackagingType = "AL",
                PackagingTonnage = 50,
                SubmissionPeriod = subPeriod,
                Material = new Material
                {
                    Id = 1,
                    Code = "HH",
                    Name = "Material1",
                    Description = "Material1"
                }
            });

            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                MaterialId = 3,
                ProducerDetailId = 2,
                PackagingType = "HDC",
                PackagingTonnage = 50,
                SubmissionPeriod = subPeriod,
                Material = new Material
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Material2",
                    Description = "Material2"
                }
            });

            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                MaterialId = 1,
                ProducerDetailId = 3,
                PackagingType = "PB",
                PackagingTonnage = 100,
                SubmissionPeriod = subPeriod,
                Material = new Material
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Material1",
                    Description = "Material1"
                }
            });
        }

        return producers;
    }

    private static MaterialDetail GetMaterial()
    {
        var material = new MaterialDetail
        {
            Id = 1,
            Code = "AL",
            Name = "Material1"
        };
        return material;
    }

    private static MaterialDetail GetHDCMaterial()
    {
        var material = new MaterialDetail
        {
            Id = 3,
            Code = "GL",
            Name = "Material2"
        };
        return material;
    }

    private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage() => Fixture.Create<CalcResultLateReportingTonnage>();
}
