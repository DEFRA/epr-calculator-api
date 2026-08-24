using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Billing;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunProcessorTests : TestsFor<BillingRunProcessor>
{
    private Mock<IBillingRunFinalizer> finalizer = null!;
    private Mock<ILogger<BillingRunProcessor>> logger = null!;
    private BillingRunContext runContext = null!;

    protected override void TestInitialize()
    {
        runContext = TestDataHelper.BillingRun2025;
        finalizer = fixture.Freeze<Mock<IBillingRunFinalizer>>();
        logger = fixture.Freeze<Mock<ILogger<BillingRunProcessor>>>();

        dbContext.ProducerDisposalFee.Add(new ProducerFees
        {
            CalculatorRunId = runContext.RunId,
            Total = new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty }
        });
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
        finalizer
            .Setup(f => f.FinalizeAsCompleted(runContext, It.IsAny<ProducerFees>(), CancellationToken.None))
            .ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "cancellation");
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        var exception = new Exception("Test failure");
        finalizer
            .Setup(f => f.FinalizeAsCompleted(runContext, It.IsAny<ProducerFees>(), CancellationToken.None))
            .ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "failed");
    }

    [TestMethod]
    public async Task Should_handle_missing_producer_fees()
    {
        dbContext.ProducerDisposalFee.RemoveRange(dbContext.ProducerDisposalFee);
        dbContext.SaveChanges();

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "failed");
        finalizer.Verify(f => f.FinalizeAsErrored(runContext, CancellationToken.None), Times.Once);
    }
}
