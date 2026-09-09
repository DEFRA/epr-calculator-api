using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using Microsoft.Extensions.Time.Testing;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Calculator;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class HistoricalCalculatorRunReprocessorTests : TestsFor<HistoricalCalculatorRunReprocessor>
{
    private Mock<IResultBuilder> resultBuilder = null!;
    private Mock<IParameterService> parameterService = null!;
    private FakeTimeProvider timeProvider = null!;
    private DefaultParameters defaultParameters = null!;

    protected override void TestInitialize()
    {
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = 1,
            Name = "Historical run",
            RelativeYear = new RelativeYear(2025),
            CreatedBy = "Original User",
            CalculatorRunClassificationId = RunClassificationStatusIds.FINALRUNCOMPLETEDID
        });

        dbContext.SaveChanges();

        resultBuilder = fixture.Freeze<Mock<IResultBuilder>>();

        defaultParameters = fixture.Create<DefaultParameters>();
        parameterService = fixture.Freeze<Mock<IParameterService>>();
        parameterService.Setup(p => p.GetDefaultParameters(1)).ReturnsAsync(defaultParameters);

        timeProvider = (FakeTimeProvider)fixture.Freeze<TimeProvider>();
    }

    [TestMethod]
    public async Task Should_build_context_from_existing_run_and_delegate_to_result_builder()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(now);
        var expected = fixture.Create<CalcResult>();
        resultBuilder.Setup(b => b.BuildAsync(It.IsAny<CalculatorRunContext>(), CancellationToken.None)).ReturnsAsync(expected);

        var result = await testSubject.ReprocessAsync(1, CancellationToken.None);

        result.ShouldBe(expected);

        resultBuilder.Verify(b => b.BuildAsync(
            It.Is<CalculatorRunContext>(c =>
                c.RunId == 1 &&
                c.RunName == "Historical run" &&
                c.RelativeYear == new RelativeYear(2025) &&
                c.User == "Original User" &&
                c.ProcessingStartedAt == now &&
                c.DefaultParameters == defaultParameters &&
                c.RunType == RunType.Calculator),
            CancellationToken.None));
    }

    [TestMethod]
    public async Task Should_throw_when_run_not_found()
    {
        await Should.ThrowAsync<InvalidOperationException>(async () => await testSubject.ReprocessAsync(2, CancellationToken.None));
    }
}
