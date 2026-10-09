using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Features.Calculator;

[TestCategory(TestCategories.CalculatorRuns)]
[TestClass]
public class HistoricalCalculatorRunReprocessorTests
{
    private const int RunNeedingReprocessingId = 1;
    private const int AlreadyProcessedRunId = 2;
    private const int OtherRunNeedingReprocessingId = 3;

    private static readonly RelativeYear RelativeYear = new(2025);

    private IFixture fixture = null!;
    private SqliteConnection connection = null!;
    private ApplicationDBContext dbContext = null!;
    private Mock<IResultBuilder> resultBuilder = null!;
    private Mock<IParameterService> parameterService = null!;
    private FakeTimeProvider timeProvider = null!;
    private HistoricalCalculatorRunReprocessor testSubject = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        dbContext = new ApplicationDBContext(new DbContextOptionsBuilder<ApplicationDBContext>().UseSqlite(connection).Options);
        dbContext.Database.EnsureCreated();

        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = RelativeYear });
        dbContext.CalculatorRuns.AddRange(
            new CalculatorRun { Id = RunNeedingReprocessingId, Name = " Historical run ", RelativeYear = RelativeYear, CreatedBy = "Original User", Classification = RunClassification.InitialCompleted },
            new CalculatorRun { Id = AlreadyProcessedRunId, Name = "Processed run", RelativeYear = RelativeYear, CreatedBy = "Original User", Classification = RunClassification.InitialCompleted },
            new CalculatorRun { Id = OtherRunNeedingReprocessingId, Name = "Other historical run", RelativeYear = RelativeYear, CreatedBy = "Original User", Classification = RunClassification.RecalculationCompleted });

        var existingFees = TestDataHelper.GetProducerFees();
        existingFees.CalculatorRunId = AlreadyProcessedRunId;
        dbContext.ProducerDisposalFee.Add(existingFees);

        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        fixture = TestFixtures.New();
        fixture.Inject(dbContext);
        resultBuilder = fixture.Freeze<Mock<IResultBuilder>>();
        parameterService = fixture.Freeze<Mock<IParameterService>>();
        timeProvider = (FakeTimeProvider)fixture.Freeze<TimeProvider>();
        testSubject = fixture.Create<HistoricalCalculatorRunReprocessor>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        dbContext.Dispose();
        connection.Dispose();
    }

    [TestMethod]
    public async Task Should_only_reprocess_runs_without_producer_fees()
    {
        await testSubject.ReprocessAsync(CancellationToken.None);

        resultBuilder.Verify(b => b.BuildAsync(It.Is<CalculatorRunContext>(c => c.RunId == RunNeedingReprocessingId), It.IsAny<CancellationToken>()), Times.Once);
        resultBuilder.Verify(b => b.BuildAsync(It.Is<CalculatorRunContext>(c => c.RunId == OtherRunNeedingReprocessingId), It.IsAny<CancellationToken>()), Times.Once);
        resultBuilder.Verify(b => b.BuildAsync(It.Is<CalculatorRunContext>(c => c.RunId == AlreadyProcessedRunId), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Should_build_context_from_existing_run()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(now);
        var defaultParameters = fixture.Create<DefaultParameters>();
        parameterService.Setup(p => p.GetDefaultParameters(RunNeedingReprocessingId)).ReturnsAsync(defaultParameters);

        await testSubject.ReprocessAsync(CancellationToken.None);

        resultBuilder.Verify(b => b.BuildAsync(
            It.Is<CalculatorRunContext>(c =>
                c.RunId == RunNeedingReprocessingId &&
                c.RunName == "Historical run" &&
                c.RelativeYear == RelativeYear &&
                c.User == "Original User" &&
                c.ProcessingStartedAt == now &&
                c.DefaultParameters == defaultParameters &&
                c.RunType == RunType.Calculator),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Should_delete_existing_results_for_reprocessed_runs_only()
    {
        SeedTransformPartial(RunNeedingReprocessingId);
        SeedTransformPartial(AlreadyProcessedRunId);

        await testSubject.ReprocessAsync(CancellationToken.None);

        (await CountTransformPartial(RunNeedingReprocessingId)).ShouldBe(0);
        (await CountTransformPartial(AlreadyProcessedRunId)).ShouldBe(1);
        (await dbContext.ProducerDisposalFee.CountAsync(f => f.CalculatorRunId == AlreadyProcessedRunId)).ShouldBe(1);
    }

    [TestMethod]
    public async Task Should_roll_back_deletes_and_continue_when_a_run_fails()
    {
        SeedTransformPartial(RunNeedingReprocessingId);
        resultBuilder
            .Setup(b => b.BuildAsync(It.Is<CalculatorRunContext>(c => c.RunId == RunNeedingReprocessingId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test failure"));

        await testSubject.ReprocessAsync(CancellationToken.None);

        (await CountTransformPartial(RunNeedingReprocessingId)).ShouldBe(1);
        resultBuilder.Verify(b => b.BuildAsync(It.Is<CalculatorRunContext>(c => c.RunId == OtherRunNeedingReprocessingId), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SeedTransformPartial(int runId)
    {
        dbContext.TransformPartial.Add(fixture.Build<TransformPartial>()
            .Without(t => t.Id)
            .With(t => t.CalculatorRunId, runId)
            .Create());

        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();
    }

    private Task<int> CountTransformPartial(int runId) =>
        dbContext.TransformPartial.AsNoTracking().CountAsync(t => t.CalculatorRunId == runId);
}
