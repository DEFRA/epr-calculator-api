using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class BillingInstructionServiceTests
{
    private IFixture fixture = null!;
    private BillingInstructionService sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        fixture = TestFixtures.New();

        sut = fixture.Create<BillingInstructionService>();
    }

    [TestMethod]
    public async Task CanCallCreateBillingInstructions()
    {
        // Arrange
        var calcResult = TestDataHelper.GetCalcResult();

        // Act
        var result = await sut.CreateBillingInstructions(calcResult);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CanCallCreateBillingInstructionsWithNoInstructions()
    {
        // Arrange
        var calcResult = new CalcResult
        {
            ApplyModulation = true,
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
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
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };

        // Act
        var result = await sut.CreateBillingInstructions(calcResult);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanCallCreateBillingInstructionsWithNoProducers()
    {
        // Arrange
        var calcResult = new CalcResult
        {
            ApplyModulation = true,
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
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
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };

        // Act
        var result = await sut.CreateBillingInstructions(calcResult);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanCallCreateBillingInstructionsWithCancelledProducers()
    {
        // Arrange
        var calcResult = new CalcResult
        {
            ApplyModulation = true,
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
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
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };

        // Act
        var result = await sut.CreateBillingInstructions(calcResult);
        Assert.IsTrue(result);
    }
}
