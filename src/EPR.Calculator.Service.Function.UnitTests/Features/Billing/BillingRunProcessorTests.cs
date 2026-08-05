using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRuns.Outputs;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunProcessorTests : TestsFor<BillingRunProcessor>
{
    private const int AcceptedProducerId = 1;
    private const int RejectedProducerId = 42;
    private Mock<IBillingBuilder> builder = null!;
    private Mock<IBillingFileGenerator> fileGenerator = null!;
    private Mock<IBillingRunFinalizer> finalizer = null!;
    private Mock<ILogger<BillingRunProcessor>> logger = null!;
    private BillingRunContext runContext = null!;

    protected override void TestInitialize()
    {
        runContext = TestDataHelper.BillingRun2025;
        builder = fixture.Freeze<Mock<IBillingBuilder>>();
        fileGenerator = fixture.Freeze<Mock<IBillingFileGenerator>>();
        finalizer = fixture.Freeze<Mock<IBillingRunFinalizer>>();
        logger = fixture.Freeze<Mock<ILogger<BillingRunProcessor>>>();

        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestDataHelper.GetCalcResult());
    }

    [TestMethod]
    public async Task Should_handle_success()
    {
        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Should_handle_cancelled()
    {
        var exception = new OperationCanceledException("Test cancelled");
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>(), CancellationToken.None)).ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "cancellation");
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        var exception = new Exception("Test failure");
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>(), CancellationToken.None)).ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "failed");
    }

    [TestMethod]
    public async Task Should_filter_accepted_producers()
    {
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCalcResult());

        CalcResult? exported = null;
        fileGenerator
            .Setup(f => f.SerializeAndExport(runContext, It.IsAny<CalcResult>(), It.IsAny<CancellationToken>()))
            .Callback<BillingRunContext, CalcResult, CancellationToken>((_, calcResult, _) => exported = calcResult)
            .ReturnsAsync((BillingFileResult?)null!);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        exported.ShouldNotBeNull();
        exported.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId)
            .ShouldBe([AcceptedProducerId]);
        exported.CalcResultPartialObligations.PartialObligations.Select(p => p.ProducerId)
            .ShouldBe([AcceptedProducerId]);
        exported.CalcResultProjectedProducers.H1ProjectedProducers.Select(p => p.ProducerId)
            .ShouldBe([AcceptedProducerId]);
        exported.CalcResultProjectedProducers.H2ProjectedProducers.Select(p => p.ProducerId)
            .ShouldBe([AcceptedProducerId]);
        exported.ProducerFees.Details.Select(p => p.FeeDetail.ProducerId)
            .ShouldBe([AcceptedProducerId]);
    }

    [TestMethod]
    public async Task Should_filter_rejected_producers()
    {
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCalcResult());

        CalcResult? exported = null;
        fileGenerator
            .Setup(f => f.SerializeAndExport(runContext, It.IsAny<CalcResult>(), It.IsAny<CancellationToken>()))
            .Callback<BillingRunContext, CalcResult, CancellationToken>((_, calcResult, _) => exported = calcResult)
            .ReturnsAsync((BillingFileResult?)null!);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        exported.ShouldNotBeNull();
        exported.CalcResultCancelledProducers.Select(p => p.ProducerId)
            .ShouldBe([AcceptedProducerId]);
    }

    private static CalcResult BuildCalcResult()
    {
        var scaledupProducers = ImmutableList.Create(
            new CalcResultScaledupProducer { ProducerId = AcceptedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" },
            new CalcResultScaledupProducer { ProducerId = RejectedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" });

        var partialObligations = ImmutableList.Create(
            new CalcResultPartialObligation { ProducerId = AcceptedProducerId, Level = "1", SubmissionYear = 2024, DaysInSubmissionYear = 366, ObligatedFactor = 0.5m },
            new CalcResultPartialObligation { ProducerId = RejectedProducerId, Level = "1", SubmissionYear = 2024, DaysInSubmissionYear = 366, ObligatedFactor = 0.5m });

        var h1ProjectedProducers = ImmutableList.Create(
            new CalcResultH1ProjectedProducer { ProducerId = AcceptedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" },
            new CalcResultH1ProjectedProducer { ProducerId = RejectedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" });

        var h2ProjectedProducers = ImmutableList.Create(
            new CalcResultH2ProjectedProducer { ProducerId = AcceptedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" },
            new CalcResultH2ProjectedProducer { ProducerId = RejectedProducerId, Level = "1", SubmissionPeriodCode = "2024-P2" });

        var producerFeeDetails = new List<ProducerFeeDetail>
        {
            new() { FeeDetail = new FeeDetail { ProducerId = AcceptedProducerId, SubsidiaryId = string.Empty, ProducerName = "Accepted Producer" } },
            new() { FeeDetail = new FeeDetail { ProducerId = RejectedProducerId, SubsidiaryId = string.Empty, ProducerName = "Rejected Producer" } }
        };

        var cancelledProducers = ImmutableList.Create(
            new CalcResultCancelledProducer { ProducerId = AcceptedProducerId, ProducerOrSubsidiaryName = "Accepted Producer", TradingName = "Accepted Trading" },
            new CalcResultCancelledProducer { ProducerId = RejectedProducerId, ProducerOrSubsidiaryName = "Rejected Producer", TradingName = "Rejected Trading" });

        var rejectedProducers = ImmutableList.Create(
            new CalcResultRejectedProducer
            {
                ProducerId = RejectedProducerId,
                ProducerName = "Rejected Producer",
                TradingName = "Rejected Trading",
                SuggestedBillingInstruction = "Invoice",
                InstructionConfirmedBy = "Test User",
                ReasonForRejection = "Test rejection"
            });

        return TestDataHelper.GetCalcResult() with
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers { ScaledupProducers = scaledupProducers },
            CalcResultPartialObligations = new CalcResultPartialObligations { PartialObligations = partialObligations },
            CalcResultProjectedProducers = new CalcResultProjectedProducers
            {
                H1ProjectedProducers = h1ProjectedProducers,
                H2ProjectedProducers = h2ProjectedProducers
            },
            ProducerFees = new ProducerFees
            {
                CalculatorRunId = 0,
                Details = producerFeeDetails,
                Total = new FeeDetail
                {
                    ProducerId = 0,
                    SubsidiaryId = string.Empty,
                    ProducerName = string.Empty,
                    TotalOnePlus2A2B2CWithBadDebtPercentage = 123.45m
                }
            },
            CalcResultCancelledProducers = cancelledProducers,
            CalcResultRejectedProducers = rejectedProducers
        };
    }
}
