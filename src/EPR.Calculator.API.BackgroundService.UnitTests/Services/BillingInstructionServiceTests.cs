using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

[TestClass]
public class BillingInstructionServiceTests : TestsFor<BillingInstructionService>
{
    [TestMethod]
    public async Task Should_create_instructions()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();
        var producerFeeDetails = TestDataHelper.GetProducerFeeDetails();

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateBillingInstructions(runContext, calcResult, producerFeeDetails, CancellationToken.None));
    }

    [TestMethod]
    public async Task Should_create_instructions_with_cancelled_producers()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations
            {
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
                ByMaterial = new Dictionary<string, ByCountryCost>()
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = ByCountryCost.Empty
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>()
            },
            CalcResultCancelledProducers = new List<CalcResultCancelledProducer>
            {
                new()
                {
                    LastTonnage = null,
                    ProducerId = 1,
                    TradingName = "Test",
                    LatestInvoice = new LatestInvoice
                    {
                        BillingInstructionId = "1_1",
                        RunName = "RunName",
                        RunNumber = "4"
                    }
                }
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers
            {
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty
            }
        };

        var producerFeeDetails = fixture.Create<List<ProducerFeeDetail>>().Select(d => d.FeeDetail).ToList();

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateBillingInstructions(runContext, calcResult, producerFeeDetails, CancellationToken.None));
    }
}
