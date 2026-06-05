using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.BillingRun;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Billing;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingRunProcessorTests : TestsFor<BillingRunProcessor>
{
    private Mock<ICalcResultBuilder> builder = null!;
    private Mock<ILogger<BillingRunProcessor>> logger = null!;
    private BillingRunContext runContext = null!;

    protected override void TestInitialize()
    {
        runContext = TestDataHelper.BillingRun2025;
        builder = fixture.Freeze<Mock<ICalcResultBuilder>>();
        logger = fixture.Freeze<Mock<ILogger<BillingRunProcessor>>>();
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
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "cancellation");
    }

    [TestMethod]
    public async Task Should_handle_failure()
    {
        var exception = new Exception("Test failure");
        builder.Setup(b => b.BuildAsync(It.IsAny<RunContext>())).ThrowsAsync(exception);

        var result = await testSubject.Process(runContext, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        logger.VerifyLogContains(LogLevel.Error, "failed");
    }
}
