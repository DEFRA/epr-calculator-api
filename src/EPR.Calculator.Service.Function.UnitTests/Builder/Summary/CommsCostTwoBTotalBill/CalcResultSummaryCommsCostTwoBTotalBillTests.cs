using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.CommsCostTwoBTotalBill;

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
            CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
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
                ProducerReportedMaterialProjected =
                    new ProducerReportedMaterialProjected
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
                ProducerReportedMaterialProjected =
                    new ProducerReportedMaterialProjected
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
                ProducerReportedMaterialProjected =
                    new ProducerReportedMaterialProjected
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
                ProducerReportedMaterialProjected =
                    new ProducerReportedMaterialProjected
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
        TotalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(allResults, materials, 1);
    }

    private Fixture Fixture { get; } = new();

    [TestCleanup]
    public void TestCleanup()
    {
        producers = null!;
        calcResult = null!;
        allResults = null!;
    }

    [TestMethod]
    public void GetCommsProducerFeeWithBadDebtFor2bTotalsRow_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producers, TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(278.41m, result);
    }

    [TestMethod]
    public void GetCommsEnglandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producers, TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(111.364m, result);
    }

    [TestMethod]
    public void GetCommsNorthernIrelandWithBadDebtTotalsRow_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producers, TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(41.7615m, result);
    }

    [TestMethod]
    public void GetCommsEnglandWithBadDebt_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(111.364m, result);
    }

    [TestMethod]
    public void GetCommsWalesWithBadDebt_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(83.523m, result);
    }

    [TestMethod]
    public void GetCommsScotlandWithBadDebt_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(41.7615m, result);
    }

    [TestMethod]
    public void GetCommsNorthernIrelandWithBadDebt_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(41.7615m, result);
    }

    [TestMethod]
    public void GetCommsWithBadDebt_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWithBadDebt(calcResult, producers[0], TotalPackagingTonnage, "England");

        // Assert
        Assert.AreEqual(111.364m, result);
    }

    [TestMethod]
    public void GetRegionApportionment_ShouldReturnCorrectValue()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetRegionApportionment(calcResult, "England");

        // Assert
        Assert.AreEqual(40, result);
    }

    [TestMethod]
    public void GetCommsBadDebtProvisionFor2b_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(25.31m, result);
    }

    [TestMethod]
    public void GetCommsProducerFeeWithBadDebtFor2b_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(278.41m, result);
    }

    [TestMethod]
    public void CalculateProducerFee_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.CalculateProducerFee(calcResult, producers[0], TotalPackagingTonnage, false);

        // Assert
        Assert.AreEqual(253.1m, result);
    }

    [TestMethod]
    public void GetCommsProducerFeeWithoutBadDebtFor2b_ShouldReturnCorrectTotal()
    {
        // Act
        var result = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producers[0], TotalPackagingTonnage);

        // Assert
        Assert.AreEqual(253.1m, result);
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