using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

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
            ProducerFees = new ProducerFees
            {
                CalculatorRunId = 0, 
                Details = fixture.Create<List<ProducerFeeDetail>>(),
                Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty }
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

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateBillingInstructions(runContext, calcResult));
    }

    [TestMethod]
    public async Task Should_throw_when_no_producers()
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
            ProducerFees = new ProducerFees {
                CalculatorRunId = 0, 
                Details = null!,
                Total = new() { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty }
            },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty
            }
        };

        // Act & Assert
        await Should.ThrowAsync<RunProcessingException>(testSubject.CreateBillingInstructions(runContext, calcResult));
    }
}
