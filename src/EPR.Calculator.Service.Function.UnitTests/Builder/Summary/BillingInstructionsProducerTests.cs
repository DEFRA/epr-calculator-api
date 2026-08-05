using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

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

    [TestMethod]
    public void BillingInstructionsProducer_CanCallSetValues()
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

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 99999m, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = -99999m, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, producerInvoicedMaterialNetTonnage, otherCost);
        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction;

        var calcTotal = calcResult.ProducerFees.Details.First().FeeDetail.TotalBillBreakdown!.ByCountry.Total;
        var expectedLiabilityDiff = MathUtils.RoundAwayFromZero(calcTotal, 2) - MathUtils.RoundAwayFromZero(20.00m, 2);

        // Assert
        Assert.AreEqual(20.00m, fee!.CurrentYearInvoiceTotalToDate);
        Assert.AreEqual(null, fee.TonnageChangeSinceLastInvoice);
        Assert.AreEqual(expectedLiabilityDiff, fee.LiabilityDifference);
        Assert.AreEqual(null, fee.MaterialityLiabilityDirection);
        Assert.AreEqual(null, fee.TonnageAmountLiabilityDirection);
        Assert.AreEqual(52355.85m, fee.PercentageLiabilityDifference);
        Assert.AreEqual(LiabilityDirection.Negative, fee.MaterialityPercentageLiabilityDirection);
        Assert.AreEqual(null, fee.TonnageAmountPercentageLiabilityDirection);
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
        Assert.AreEqual(10471.17m, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Total = TestDataHelper.GetOverallTotalRow(),
            Details =
            [
                new ProducerFeeDetail
                {
                    FeeDetail = new FeeDetail
                    {
                        ProducerId = 101,
                        SubsidiaryId = "1000",
                        Level = "1",
                        ProducerName = "P1",
                        TotalBillBreakdown = new FeeWithBadDebt
                        {
                            ByCountry = new ByCountryCost { England = 120.004m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
                        }
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

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());

        var fee = producerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        var expected = MathUtils.RoundAwayFromZero(120.004m, 2) - MathUtils.RoundAwayFromZero(20.003m, 2);
        Assert.AreEqual(expected, fee.LiabilityDifference);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_LevelNot1_ReturnsNull()
    {
        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Total = TestDataHelper.GetOverallTotalRow(),
            Details = (List<ProducerFeeDetail>)
            [
                new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 301,
                    SubsidiaryId = "3000",
                    Level = "2",
                    ProducerName = "P3",
                    TotalBillBreakdown = new FeeWithBadDebt
                    {
                        ByCountry = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
                    }
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

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());
        Assert.IsNull(producerFees.Details.ToList()[0].FeeDetail.BillingInstruction!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_NonTotalsRow_PassesThroughCalculatedValue()
    {
        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Total = TestDataHelper.GetOverallTotalRow(),
            Details = (List<ProducerFeeDetail>)
            [
                new ProducerFeeDetail
                {
                    FeeDetail = new FeeDetail
                    {
                        ProducerId = 11,
                        SubsidiaryId = "S-11",
                        Level = "1",
                        ProducerName = "P11",
                        TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 20m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
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
                ProducerId = 11,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 5m
            }
        ];

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());

        Assert.AreEqual(15m, producerFees.Details.ToList()[0].FeeDetail.BillingInstruction!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithNonZeroRunningTotal_ReturnsSum()
    {
        var a = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };

        var b = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 2,
                    SubsidiaryId = "S-2",
                    Level = "1",
                    ProducerName = "P2",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 70m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };

        var total = new FeeDetail
            {
                ProducerId = 0,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { a, b },
            Total = total
        };

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

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());

        var d1 = MathUtils.RoundAwayFromZero(50m, 2) - MathUtils.RoundAwayFromZero(20m, 2);
        var d2 = MathUtils.RoundAwayFromZero(70m, 2) - MathUtils.RoundAwayFromZero(80m, 2);
        Assert.AreEqual(d1 + d2, producerFees.Total!.BillingInstruction!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithZeroRunningTotal_ReturnsNull()
    {
        var a = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };

        var b = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 2,
                    SubsidiaryId = "S-2",
                    Level = "1",
                    ProducerName = "P2",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 20m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };

        var total = new FeeDetail
            {
                ProducerId = 0,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { a, b },
            Total = total
        };

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

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());

        Assert.IsNull(producerFees.Total!.BillingInstruction!.LiabilityDifference);
    }


    [TestMethod]
    public void GetMaterialThresholdBreached_TotalsRow_ReturnsEmpty()
    {
        var total = new FeeDetail
            {
                ProducerId = 0,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

        var producerFees = new ProducerFees
        {
            CalculatorRunId = 0,
            Total = total
        };

        BillingInstructionsProducer.SetValues(producerFees, new List<InvoicedProducer>(), new CalcResultParameterOtherCost());

        Assert.AreEqual(null, producerFees.Total!.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NonLevel1_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "2",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 100m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        BillingInstructionsProducer.SetValues(producerFees, invoiced, new CalcResultParameterOtherCost());

        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NoLiabilityDifference_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt
            {
                ByCountry = new ByCountryCost { England = 90m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
            }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(
            producerFees,
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
            otherCost);

        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffWithinThresholds_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 200m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -200m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 150m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(LiabilityDirection.Positive, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 40m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(LiabilityDirection.Negative, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 115m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.MaterialityLiabilityDirection);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_NoChange_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TonnageChangeAdvice = "NOCHANGE",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.TonnageAmountLiabilityDirection);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffWithinThresholds_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TonnageChangeAdvice = "CHANGE",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 200m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -200m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.TonnageAmountLiabilityDirection);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TonnageChangeAdvice = "CHANGE",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 160m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(LiabilityDirection.Positive, fee.FeeDetail.BillingInstruction!.TonnageAmountLiabilityDirection);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TonnageChangeAdvice = "CHANGE",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 40m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(LiabilityDirection.Negative, fee.FeeDetail.BillingInstruction!.TonnageAmountLiabilityDirection);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new ProducerFeeDetail
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = "S-1",
                    Level = "1",
                    ProducerName = "P1",
                    TonnageChangeAdvice = "CHANGE",
                    TotalBillBreakdown = new FeeWithBadDebt { ByCountry = new ByCountryCost { England = 110m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            };
        var producerFees = new ProducerFees {
            CalculatorRunId = 0,
            Details = new List<ProducerFeeDetail> { fee },
            Total = TestDataHelper.GetOverallTotalRow()
        };

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

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(producerFees, invoiced, otherCost);
        Assert.AreEqual(null, fee.FeeDetail.BillingInstruction!.TonnageAmountLiabilityDirection);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_LevelNot1_ReturnsNull()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(52348.00m, fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Total_ReturnsNull()
    {
        var producerFees = TestDataHelper.GetProducerFees();

        BillingInstructionsProducer.SetValues(producerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        Assert.IsNull(producerFees.Total!.BillingInstruction!.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(null, fee.MaterialityPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsPositive()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(LiabilityDirection.Positive, fee.MaterialityPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsNegative()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(LiabilityDirection.Negative, fee.MaterialityPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsHypen()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = -99999m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(null, fee.MaterialityPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(null, fee.TonnageAmountPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_WhenTonnageChangeIsNull_ReturnsHypen()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(null, fee.TonnageAmountPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsPositive()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.TonnageChangeAdvice = "CHANGE";

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(LiabilityDirection.Positive, fee.TonnageAmountPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsNegative()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.TonnageChangeAdvice = "CHANGE";

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            TonnageChangeDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(LiabilityDirection.Negative, fee.TonnageAmountPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsHypen()
    {
        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(null, fee.TonnageAmountPercentageLiabilityDirection);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_NotLevel1_ReturnsHypen()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsDelta()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsRebill()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(BillingConstants.Suggestion.Rebill, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_NotLevel1_ReturnsHypen()
    {
        calcResult.ProducerFees.Details.First().FeeDetail.Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsLiabilityDifference()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(fee.LiabilityDifference, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsTotalProducerFeeWithBadDebtProvision()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], otherCost);

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.AreEqual(10491.17m, MathUtils.RoundAwayFromZero(fee.SuggestedInvoiceAmount ?? 0m, 2));
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.ProducerFees, [invoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.ProducerFees.Details.ToList()[0].FeeDetail.BillingInstruction!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }
}
