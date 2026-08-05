using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultSummaryCommsCostTwoBTotalBillTests
{
    private List<CalcResultProducerAndReportMaterialDetail> allResults;
    private CalcResult calcResult;
    private List<ProducerDetail> producers;
    public required IReadOnlyList<TotalPackagingTonnagePerRun> TotalPackagingTonnage;

    public CalcResultSummaryCommsCostTwoBTotalBillTests()
    {
        producers = GetProducers();

        calcResult = new CalcResult
        {
            CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
            CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
            CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
            CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
            CalcResultOnePlusFourApportionment = GetCalcResultOnePlusFourApportionment(),
            ProducerFees = TestDataHelper.GetProducerFees(),
            CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
            CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };

        // Set up consistent data
        calcResult.CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>();
        calcResult.CalcResultParameterOtherCost.BadDebtValue = 10;
        var producer1 = new ProducerDetail
        {
            Id = 1,
            CalculatorRunId = 1,
            SubsidiaryId = "1",
            ProducerId = 1,
            ProducerName = "Producer1"
        };
        var producer2 = new ProducerDetail
        {
            Id = 2,
            CalculatorRunId = 1,
            SubsidiaryId = "1",
            ProducerId = 2,
            ProducerName = "Producer2"
        };
        allResults = new List<CalcResultProducerAndReportMaterialDetail>
        {
            new()
            {
                ProducerDetail = producer1,
                ProducerMaterialPackaging =
                    new ProducerMaterialPackaging
                    {
                        MaterialId = 1,
                        ProducerDetailId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 50,
                        SubmissionPeriod = "2025-H1",
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
            },
            new()
            {
                ProducerDetail = producer1,
                ProducerMaterialPackaging =
                    new ProducerMaterialPackaging
                    {
                        MaterialId = 1,
                        ProducerDetailId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 50,
                        SubmissionPeriod = "2025-H2",
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
            },
            new()
            {
                ProducerDetail = producer2,
                ProducerMaterialPackaging =
                    new ProducerMaterialPackaging
                    {
                        MaterialId = 1,
                        ProducerDetailId = 2,
                        PackagingType = "HH",
                        PackagingTonnage = 450,
                        SubmissionPeriod = "2025-H1",
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
            },
            new()
            {
                ProducerDetail = producer2,
                ProducerMaterialPackaging =
                    new ProducerMaterialPackaging
                    {
                        MaterialId = 1,
                        ProducerDetailId = 2,
                        PackagingType = "HH",
                        PackagingTonnage = 450,
                        SubmissionPeriod = "2025-H2",
                        Material = new Material
                        {
                            Id = 1,
                            Code = "HH",
                            Name = "Material1",
                            Description = "Material1"
                        }
                    }
            }
        };

        var materials = TestDataHelper.GetMaterialDetails();
        TotalPackagingTonnage = ProducerFeesBuilder.GetTotalPackagingTonnagePerRun(allResults, materials, 1);
    }

    private IFixture Fixture { get; } = TestFixtures.New();

    [TestCleanup]
    public void TestCleanup()
    {
        producers = null!;
        calcResult = null!;
        allResults = null!;
    }

    [TestMethod]
    public void GetCommsCosts_ShouldReturnCorrectValues()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsCosts(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(253.1m,   result.FeeWithoutBadDebt);
        Assert.AreEqual(25.31m,   result.BadDebt);
        Assert.AreEqual(111.364m, result.ByCountry.England);
        Assert.AreEqual(83.523m,  result.ByCountry.Wales);
        Assert.AreEqual(41.7615m, result.ByCountry.Scotland);
        Assert.AreEqual(41.7615m, result.ByCountry.NorthernIreland);
    }

    private List<ProducerDetail> GetProducers()
    {
        var producers = Fixture.CreateMany<ProducerDetail>(2).ToList();
        producers[0].SubsidiaryId = "1";
        producers[0].CalculatorRunId = 1;
        producers[0].ProducerId = 1;

        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
            {
                MaterialId = 1,
                ProducerDetailId = 1,
                PackagingType = "HH",
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
        }

        return producers;
    }

    private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage() => Fixture.Create<CalcResultLateReportingTonnage>();

    private CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
    {
        return new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost = new ByCountryCost
            {
                England = 40,
                Wales = 30,
                Scotland = 15,
                NorthernIreland = 15
            },
            LADataPrepCharge = ByCountryCost.Empty
        };
    }
}
