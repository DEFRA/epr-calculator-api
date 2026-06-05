using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.UnitTests.Features.Calculator;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class CalculatorRunProcessorTests : TestsFor<CalculatorRunProcessor>
{
    private Mock<ICalcResultBuilder> builder = null!;
    private Mock<ILogger<CalculatorRunProcessor>> logger = null!;
    private CalculatorRunContext runContext = null!;

    protected override void TestInitialize()
    {
        runContext = fixture.Create<CalculatorRunContext>();
        builder = fixture.Freeze<Mock<ICalcResultBuilder>>();
        logger = fixture.Freeze<Mock<ILogger<CalculatorRunProcessor>>>();

        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runContext.RunId,
            RelativeYear = runContext.RelativeYear,
            Name = runContext.RunName
        });

        dbContext.SaveChanges();
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
