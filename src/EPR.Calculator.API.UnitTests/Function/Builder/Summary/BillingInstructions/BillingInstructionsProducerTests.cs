using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.BillingInstructions;

/// <summary>
///     Defines the <see cref="BillingInstructionsProducerTests" />
/// </summary>
[TestClass]
public class BillingInstructionsProducerTests
{
    /// <summary>
    ///     Defines the _calcResult
    /// </summary>
    private readonly CalcResult _calcResult;

    /// <summary>
    ///     Defines the _commsCostSummary
    /// </summary>
    private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

    /// <summary>
    ///     Defines the _dbContext
    /// </summary>
    private readonly ApplicationDBContext _dbContext;

    /// <summary>
    ///     Defines the _materialCostSummary
    /// </summary>
    private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;

    /// <summary>
    ///     Defines the _materials
    /// </summary>
    private readonly IEnumerable<MaterialDetail> _materials;

    private readonly InvoicedProducer _defaultInvoicedProducer = new()
    {
        CalculatorRunId = 0,
        CalculatorName = "ignored",
        ProducerId = 1,
        ProducerName = "ignored",
        TradingName = null,
        MaterialId = 1,
        BillingInstructionId = null,
        InvoicedNetTonnage = null,
        CurrentYearInvoicedTotalAfterThisRun = 20.003m
    };

    private readonly List<DefaultParamResultsClass> defaultParam;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillingInstructionsProducerTests" /> class.
    /// </summary>
    public BillingInstructionsProducerTests()
    {
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase("PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDBContext(dbContextOptions);
        _dbContext.Database.EnsureCreated();

        _materials = CreateMaterials();
        CreateProducerDetail();

        _calcResult = TestDataHelper.GetCalcResult();

        _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
        _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

        foreach (var material in _materials)
        {
            _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
            {
                HouseholdPackagingWasteTonnage = 1000,
                SelfManagedConsumerWasteTonnage = 90,
                NetReportedTonnage = (total: 910, red: null, amber: null, green: null),
                PricePerTonne = (total: 0.6676m, red: null, amber: null, green: null),
                ProducerDisposalFee = (total: 607.52m, red: null, amber: null, green: null),
                BadDebtProvision = 36.45m,
                ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                EnglandWithBadDebtProvision = 348.06m,
                WalesWithBadDebtProvision = 78.46m,
                ScotlandWithBadDebtProvision = 156.28m,
                NorthernIrelandWithBadDebtProvision = 61.18m
            });

            _commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
            {
                HouseholdPackagingWasteTonnage = 1000,
                PriceperTonne = 0.6676m,
                ProducerTotalCostWithoutBadDebtProvision = 607.52m,
                BadDebtProvision = 36.45m,
                ProducerTotalCostwithBadDebtProvision = 643.97m,
                EnglandWithBadDebtProvision = 348.06m,
                WalesWithBadDebtProvision = 78.46m,
                ScotlandWithBadDebtProvision = 156.28m,
                NorthernIrelandWithBadDebtProvision = 61.18m
            });
        }

        defaultParam = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "MATT-PI",
                ParameterValue = 50m,
                ParameterCategory = "Percentage Increase",
                ParameterType = "Material threshold"
            }
        };
    }

    [TestCleanup]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
    }

    [TestMethod]
    public void CanCallGetHeaders()
    {
        // Act
        var result = BillingInstructionsProducer.GetHeaders().ToList();

        var expectedResult = new List<CalcResultSummaryHeader>();
        expectedResult.AddRange([
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.CurrentYearInvoicedTotalToDate },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageChangeSinceLastInvoice },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.LiabilityDifference },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialThresholdBreached },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageThresholdBreached },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.PercentageLiabilityDifference },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialPercentageThresholdBreached },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnagePercentagThresholdBreached },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedBillingInstruction },
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedInvoiceAmount }
        ]);

        // Assert
        Assert.AreEqual(expectedResult[0].Name, result[0].Name);
        Assert.AreEqual(expectedResult[1].Name, result[1].Name);
        Assert.AreEqual(expectedResult[2].Name, result[2].Name);
        Assert.AreEqual(expectedResult[3].Name, result[3].Name);
        Assert.AreEqual(expectedResult[4].Name, result[4].Name);
        Assert.AreEqual(expectedResult[5].Name, result[5].Name);
        Assert.AreEqual(expectedResult[6].Name, result[6].Name);
    }

    [TestMethod]
    public void CanCallGetSummaryHeaders()
    {
        // Act
        var result = BillingInstructionsProducer.GetSummaryHeaders(296).ToList();

        var expectedResult = new List<CalcResultSummaryHeader>();
        expectedResult.AddRange([
            new CalcResultSummaryHeader { Name = BillingInstructionsHeader.Title, ColumnIndex = 296 }
        ]);

        // Assert
        Assert.AreEqual(expectedResult[0].Name, result[0].Name);
        Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
    }

    [TestMethod]
    public void CanCallSetValues()
    {
        // Act
        List<InvoicedProducer> producerInvoicedMaterialNetTonnage =
        [
            new()
            {
                CalculatorRunId = 101,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 77,
                BillingInstructionId = null,
                InvoicedNetTonnage = 20,
                CurrentYearInvoicedTotalAfterThisRun = 20.00m
            }
        ];

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, producerInvoicedMaterialNetTonnage, defaultParam);
        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection;

        var calcTotal = _calcResult.CalcResultSummary.ProducerDisposalFees.First().TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision;
        var expectedLiabilityDiff = Math.Round(calcTotal, 2) - Math.Round(20.00m, 2);

        // Assert
        Assert.AreEqual(20.00m, fee!.CurrentYearInvoiceTotalToDate);
        Assert.AreEqual(null, fee.TonnageChangeSinceLastInvoice);
        Assert.AreEqual(expectedLiabilityDiff, fee.LiabilityDifference);
        Assert.AreEqual("-", fee.MaterialThresholdBreached);
        Assert.AreEqual("-", fee.TonnageThresholdBreached);
        Assert.AreEqual(52355.85m, fee.PercentageLiabilityDifference);
        Assert.AreEqual("-ve", fee.MaterialPercentageThresholdBreached);
        Assert.AreEqual("-", fee.TonnagePercentageThresholdBreached);
        Assert.AreEqual(CommonConstants.Delta, fee.SuggestedBillingInstruction);
        Assert.AreEqual(10471.17m, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        var summary = new CalcResultSummary
        {
            ProducerDisposalFees =
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "101",
                    ProducerIdInt = 101,
                    Level = "1",
                    SubsidiaryId = "1000",
                    ProducerName = "P1",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithBadDebtProvision = 120.004m
                    }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 101,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20.003m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

        var fee = summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        var expected = Math.Round(120.004m, 2) - Math.Round(20.003m, 2);
        Assert.AreEqual(expected, fee.LiabilityDifference);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_LevelNot1_ReturnsNull()
    {
        var summary = new CalcResultSummary
        {
            ProducerDisposalFees = (List<CalcResultSummaryProducerDisposalFees>)
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "301",
                    ProducerIdInt = 301,
                    Level = "2",
                    SubsidiaryId = "3000",
                    ProducerName = "P3",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithBadDebtProvision = 50m
                    }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 301,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 10m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());
        Assert.IsNull(summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_NonTotalsRow_PassesThroughCalculatedValue()
    {
        var summary = new CalcResultSummary
        {
            ProducerDisposalFees = (List<CalcResultSummaryProducerDisposalFees>)
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "11",
                    ProducerIdInt = 11,
                    Level = "1",
                    SubsidiaryId = "S-11",
                    ProducerName = "P11",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 20m }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 11,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 5m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

        Assert.AreEqual(15m, summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithNonZeroRunningTotal_ReturnsSum()
    {
        var a = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 50m }
        };

        var b = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "2",
            ProducerIdInt = 2,
            Level = "1",
            SubsidiaryId = "S-2",
            ProducerName = "P2",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 70m }
        };

        var total = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = string.Empty,
            ProducerIdInt = 0,
            Level = string.Empty,
            LeaverDate = CommonConstants.Totals,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b, total } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20m
            },
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 2,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 80m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

        var fees = summary.ProducerDisposalFees.ToList();
        var totalsRow = fees[2].BillingInstructionSection!;
        var d1 = Math.Round(50m, 2) - Math.Round(20m, 2);
        var d2 = Math.Round(70m, 2) - Math.Round(80m, 2);
        Assert.AreEqual(d1 + d2, totalsRow.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithZeroRunningTotal_ReturnsNull()
    {
        var a = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 50m }
        };

        var b = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "2",
            ProducerIdInt = 2,
            Level = "1",
            SubsidiaryId = "S-2",
            ProducerName = "P2",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 20m }
        };

        var total = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = string.Empty,
            ProducerIdInt = 0,
            Level = string.Empty,
            LeaverDate = CommonConstants.Totals,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b, total } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20m
            },
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 2,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 50m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

        var totalsRow = summary.ProducerDisposalFees.ToList()[2].BillingInstructionSection!;
        Assert.IsNull(totalsRow.LiabilityDifference);
    }


    [TestMethod]
    public void GetMaterialThresholdBreached_TotalsRow_ReturnsEmpty()
    {
        var total = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = string.Empty,
            ProducerIdInt = 0,
            Level = string.Empty,
            LeaverDate = CommonConstants.Totals,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { total } };

        BillingInstructionsProducer.SetValues(summary, new List<InvoicedProducer>(), new List<DefaultParamResultsClass>());

        Assert.AreEqual(string.Empty, summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NonLevel1_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "2",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 100m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 90m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NoLiabilityDifference_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1"
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        BillingInstructionsProducer.SetValues(
            summary,
            [
                new InvoicedProducer
                {
                    CalculatorRunId = 0,
                    CalculatorName = "ignored",
                    ProducerId = 1,
                    ProducerName = "ignored",
                    TradingName = null,
                    MaterialId = 0,
                    BillingInstructionId = null,
                    InvoicedNetTonnage = null,
                    CurrentYearInvoicedTotalAfterThisRun = 90m
                }
            ],
            new List<DefaultParamResultsClass>
            {
                new()
                {
                    ParameterUniqueReference = "MATT-AI",
                    ParameterValue = 50m,
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Materiality threshold"
                },
                new()
                {
                    ParameterUniqueReference = "MATT-AD",
                    ParameterValue = -50m,
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold"
                }
            });

        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_ParamsMissing_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());
        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 150m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "MATT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold"
            },
            new()
            {
                ParameterUniqueReference = "MATT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Materiality threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("+ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 40m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "MATT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold"
            },
            new()
            {
                ParameterUniqueReference = "MATT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Materiality threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("-ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 115m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "MATT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold"
            },
            new()
            {
                ParameterUniqueReference = "MATT-AD",
                ParameterValue = -50m,
                ParameterType = "Materiality threshold",
                ParameterCategory = "Amount Decrease"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_NoChange_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "NOCHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "TONT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold"
            },
            new()
            {
                ParameterUniqueReference = "TONT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_ParamsMissing_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 160m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "TONT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold"
            },
            new()
            {
                ParameterUniqueReference = "TONT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("+ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 40m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "TONT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold"
            },
            new()
            {
                ParameterUniqueReference = "TONT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("-ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            ProducerId = "1",
            ProducerIdInt = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 110m }
        };
        var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var dp = new List<DefaultParamResultsClass>
        {
            new()
            {
                ParameterUniqueReference = "TONT-AI",
                ParameterValue = 50m,
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold"
            },
            new()
            {
                ParameterUniqueReference = "TONT-AD",
                ParameterValue = -50m,
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold"
            }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, dp);
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_LevelNot1_ReturnsNull()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(52348.00m, fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Total_ReturnsNull()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().LeaverDate = CommonConstants.Totals;

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsPositive()
    {
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsNegative()
    {
        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsHypen()
    {
        defaultParam.First().ParameterUniqueReference = "";
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_WhenTonnageChangeIsNull_ReturnsHypen()
    {
        defaultParam.First().ParameterUniqueReference = "TONT-PI";
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsPositive()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";
        defaultParam.First().ParameterUniqueReference = "TONT-PI";
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsNegative()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";
        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "TONT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsHypen()
    {
        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_NotLevel1_ReturnsHypen()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsDelta()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Delta, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsRebill()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Rebill, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsHypen()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_NotLevel1_ReturnsHypen()
    {
        _calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [_defaultInvoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsLiabilityDifference()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(fee.LiabilityDifference, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsTotalProducerFeeWithBadDebtProvision()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        _dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(new List<ProducerResultFileSuggestedBillingInstruction>
        {
            new() { Id = 1, CalculatorRunId = 101, ProducerId = 1, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
            new() { Id = 2, CalculatorRunId = 101, ProducerId = 2, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" },
            new() { Id = 3, CalculatorRunId = 101, ProducerId = 3, SuggestedBillingInstruction = "INITIAL", BillingInstructionAcceptReject = "Accepted" }
        });

        _dbContext.SaveChanges();

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(10491.17m, Math.Round(fee.SuggestedInvoiceAmount ?? 0m, 2));
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsHypen()
    {
        var invoicedProducer = _defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, [invoicedProducer], new List<DefaultParamResultsClass>());

        var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }

    private ImmutableList<MaterialDetail> CreateMaterials()
    {
        ImmutableList<Material> materials =
        [
            new() { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new() { Id = 2, Code = "FC", Name = "Fibre composite", Description = "Fibre composite" },
            new() { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
            new() { Id = 4, Code = "PC", Name = "Paper or card", Description = "Paper or card" },
            new() { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
            new() { Id = 6, Code = "ST", Name = "Steel", Description = "Steel" },
            new() { Id = 7, Code = "WD", Name = "Wood", Description = "Wood" },
            new() { Id = 8, Code = "OT", Name = "Other materials", Description = "Other materials" }
        ];

        _dbContext.Material.AddRange(materials);
        _dbContext.SaveChanges();

        return materials.Select(m => new MaterialDetail
        {
            Id = m.Id,
            Code = m.Code,
            Name = m.Name,
            Description = m.Description!
        }).ToImmutableList();
    }

    private void CreateProducerDetail()
    {
        var producerNames = new[]
        {
            "Allied Packaging",
            "Beeline Materials",
            "Cloud Boxes",
            "Decking and Shed",
            "Electric Things",
            "French Flooring",
            "Good Fruit Co",
            "Happy Shopper",
            "Icicle Foods",
            "Jumbo Box Store"
        };

        var producerId = 1;
        foreach (var producerName in producerNames)
            _dbContext.ProducerDetail.Add(new ProducerDetail
            {
                ProducerId = producerId++,
                SubsidiaryId = $"{producerId}-Sub",
                ProducerName = producerName,
                CalculatorRunId = 1
            });

        _dbContext.SaveChanges();

        for (var producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
        for (var materialId = 1; materialId < 9; materialId++)
            foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
                _dbContext.ProducerReportedMaterial.AddRange(
                    new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        SubmissionPeriod = subPeriod,
                        PackagingTonnage = materialId * 50
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "CW",
                        SubmissionPeriod = subPeriod,
                        PackagingTonnage = materialId * 25
                    });

        _dbContext.SaveChanges();
    }
}
