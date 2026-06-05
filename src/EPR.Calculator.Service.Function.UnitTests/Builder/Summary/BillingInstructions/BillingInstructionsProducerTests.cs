using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRun.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.BillingInstructions;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class BillingInstructionsProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    private readonly InvoicedProducer defaultInvoicedProducer = new()
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

    private readonly List<DefaultParamResultsClass> defaultParam =
    [
        new()
        {
            ParameterUniqueReference = "MATT-PI",
            ParameterValue = 50m,
            ParameterCategory = "Percentage Increase",
            ParameterType = "Material threshold"
        }
    ];

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

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, producerInvoicedMaterialNetTonnage, defaultParam);
        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection;

        var calcTotal = calcResult.CalcResultSummary.ProducerDisposalFees.First().TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision;
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
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
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
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(52348.00m, fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Total_ReturnsNull()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().LeaverDate = CommonConstants.Totals;

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsPositive()
    {
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsNegative()
    {
        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsHypen()
    {
        defaultParam.First().ParameterUniqueReference = "";
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_WhenTonnageChangeIsNull_ReturnsHypen()
    {
        defaultParam.First().ParameterUniqueReference = "TONT-PI";
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsPositive()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";
        defaultParam.First().ParameterUniqueReference = "TONT-PI";
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsNegative()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";
        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "TONT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsHypen()
    {
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_NotLevel1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsDelta()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsRebill()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(BillingConstants.Suggestion.Rebill, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_NotLevel1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsLiabilityDifference()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(fee.LiabilityDifference, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsTotalProducerFeeWithBadDebtProvision()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        defaultParam.First().ParameterValue = 55000m;
        defaultParam.First().ParameterUniqueReference = "MATT-PD";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], defaultParam);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(10491.17m, Math.Round(fee.SuggestedInvoiceAmount ?? 0m, 2));
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], new List<DefaultParamResultsClass>());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }
}
