using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class BillingInstructionServiceTests : TestsFor<BillingInstructionService>
{
    [TestMethod]
    public async Task Should_create_instructions()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateBillingInstructions(runContext, calcResult));
    }

     [TestMethod]
    public async Task Should_create_instructions_with_cancelled_producers()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = []
            },
            CalcResultSummary = new CalcResultSummary
            {
                ProducerDisposalFees = fixture.Create<List<CalcResultSummaryProducerDisposalFees>>()
            },
            CalcResultCancelledProducers = new CalcResultCancelledProducersResponse
            {
                TitleHeader = CommonConstants.CancelledProducers,
                CancelledProducers = new List<CalcResultCancelledProducersDto>
                {
                    new()
                    {
                        LastTonnage = null,
                        ProducerId = 1,
                        TradingNameValue = "Test",
                        LatestInvoice = new LatestInvoice
                        {
                            BillingInstructionIdValue = "1_1",
                            RunNameValue = "RunName",
                            RunNumberValue = "4"
                        }
                    }
                }
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateBillingInstructions(runContext, calcResult));
    }

    [TestMethod]
    public async Task Should_throw_when_no_instructions()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = []
            },
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
                        Level = CommonConstants.LevelTwo.ToString(),
                        BillingInstructionSection = new CalcResultSummaryBillingInstruction
                        {
                            CurrentYearInvoiceTotalToDate = 1000m,
                            TonnageChangeSinceLastInvoice = "Tonnage Changed",
                            LiabilityDifference = -200,
                            MaterialThresholdBreached = "-ve",
                            TonnageThresholdBreached = "-ve",
                            PercentageLiabilityDifference = 10.05m,
                            MaterialPercentageThresholdBreached = "-ve",
                            TonnagePercentageThresholdBreached = "-ve",
                            SuggestedBillingInstruction = "INITIAL",
                            SuggestedInvoiceAmount = 500m
                        }
                    }
                }
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };

        // Act & Assert
        await Should.ThrowAsync<RunProcessingException>(testSubject.CreateBillingInstructions(runContext, calcResult));
    }

    [TestMethod]
    public async Task Should_throw_when_no_producers()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = []
            },
            CalcResultSummary = new CalcResultSummary { ProducerDisposalFees = null! },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            }
        };

        // Act & Assert
        await Should.ThrowAsync<RunProcessingException>(testSubject.CreateBillingInstructions(runContext, calcResult));
    }
}
