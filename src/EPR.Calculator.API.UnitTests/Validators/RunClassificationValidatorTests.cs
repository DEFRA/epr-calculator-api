using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Validators;

[TestClass]
public class RunClassificationValidatorTests
{
    private static readonly RelativeYear Year2024 = new(2024);
    private static readonly RelativeYear Year2025 = new(2025);

    /// <summary>
    ///     The moment an earlier run's billing file was authorised (i.e. sent to FSS). Runs created
    ///     before this are considered outdated.
    /// </summary>
    private static readonly DateTime FssSentAt = new(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private ApplicationDBContext dbContext = null!;
    private RunClassificationValidator validator = null!;
    private int lastRunId;

    public TestContext TestContext { get; set; } = null!;

    private CancellationToken CancellationToken => TestContext.CancellationTokenSource.Token;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        dbContext = new ApplicationDBContext(options);

        dbContext.CalculatorRunRelativeYears.AddRange(
            new CalculatorRunRelativeYear { Value = Year2024 },
            new CalculatorRunRelativeYear { Value = Year2025 });

        dbContext.SaveChanges();

        validator = new RunClassificationValidator(dbContext);
    }

    [TestCleanup]
    public void TearDown() => dbContext.Dispose();

    [TestMethod]
    [DataRow(RunClassification.InitialCompleted)]
    [DataRow(RunClassification.RecalculationCompleted)]
    public async Task ValidateAsync_WhenRunHasAlreadyBeenCompleted_RejectsReclassification(RunClassification current)
    {
        // Arrange
        var run = AddRun(current, fssSentAt: FssSentAt);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBe("Cannot reclassify a run once the run is completed.");
    }

    [TestMethod]
    [DataRow(RunClassification.Running, RunClassification.Initial)]
    [DataRow(RunClassification.Initial, RunClassification.Recalculation)]
    [DataRow(RunClassification.Unclassified, RunClassification.InitialCompleted)]
    [DataRow(RunClassification.Unclassified, RunClassification.RecalculationCompleted)]
    [DataRow(RunClassification.Initial, RunClassification.RecalculationCompleted)]
    public async Task ValidateAsync_WhenTargetRequiresADifferentCurrentClassification_RejectsTransition(
        RunClassification current,
        RunClassification requested)
    {
        // Arrange
        var run = AddRun(current);

        // Act
        var result = await validator.ValidateAsync(run, requested, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe(string.Format(CommonResources.InvalidClassification, requested));
    }

    [TestMethod]
    [DataRow(RunClassification.None)]
    [DataRow(RunClassification.Running)]
    [DataRow(RunClassification.Unclassified)]
    public async Task ValidateAsync_WhenTargetIsNotAClassificationAUserCanSet_RejectsTransition(RunClassification requested)
    {
        // Arrange
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, requested, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBe("Invalid Classification");
    }

    [TestMethod]
    [DataRow(RunClassification.Unclassified)]
    [DataRow(RunClassification.Running)]
    [DataRow(RunClassification.Errored)]
    [DataRow(RunClassification.Test)]
    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    public async Task ValidateAsync_WhenDeletingARunThatIsNotAlreadyDeleted_ReturnsValid(RunClassification current)
    {
        // Arrange
        var run = AddRun(current);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenDeletingAnAlreadyDeletedRun_RejectsTransition()
    {
        // Arrange
        var run = AddRun(RunClassification.Deleted);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe(string.Format(CommonResources.InvalidClassification, RunClassification.Deleted));
    }

    [TestMethod]
    [DataRow(RunClassification.Unclassified)]
    [DataRow(RunClassification.Initial)]
    public async Task ValidateAsync_WhenMarkingACompletableRunAsATestRun_ReturnsValid(RunClassification current)
    {
        // Arrange
        var run = AddRun(current);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Test, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow(RunClassification.Test)]
    [DataRow(RunClassification.Running)]
    [DataRow(RunClassification.Errored)]
    [DataRow(RunClassification.Deleted)]
    public async Task ValidateAsync_WhenMarkingAnUnusableRunAsATestRun_RejectsTransition(RunClassification current)
    {
        // Arrange
        var run = AddRun(current);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Test, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe(string.Format(CommonResources.InvalidClassification, RunClassification.Test));
    }

    [TestMethod]
    public async Task ValidateAsync_WhenTargetIsNotDesignated_IgnoresTheStateOfTheRelativeYear()
    {
        // Arrange
        AddRun(RunClassification.Initial);
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenNoDesignatedRunsExistForTheYear_AllowsAnInitialRun()
    {
        // Arrange
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    public async Task ValidateAsync_WhenAnotherDesignatedRunIsStillOutstanding_RejectsDesignation(RunClassification requested)
    {
        // Arrange
        AddRun(RunClassification.Initial);
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, requested, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe("There are outstanding runs for '2024' that have not been completed.");
    }

    [TestMethod]
    [DataRow(RunClassification.Unclassified, RunClassification.Initial)]
    [DataRow(RunClassification.Initial, RunClassification.InitialCompleted)]
    public async Task ValidateAsync_WhenTheYearAlreadyHasACompletedInitialRun_RejectsAnotherInitialRun(
        RunClassification current,
        RunClassification requested)
    {
        // Arrange
        AddRun(RunClassification.InitialCompleted, fssSentAt: FssSentAt);
        var run = AddRun(current, createdAt: FssSentAt.AddDays(1));

        // Act
        var result = await validator.ValidateAsync(run, requested, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBe("A completed Initial run already exists for '2024'.");
    }

    [TestMethod]
    [DataRow(RunClassification.Unclassified, RunClassification.Recalculation)]
    [DataRow(RunClassification.Recalculation, RunClassification.RecalculationCompleted)]
    public async Task ValidateAsync_WhenTheYearHasNoCompletedInitialRun_RejectsRecalculation(
        RunClassification current,
        RunClassification requested)
    {
        // Arrange
        var run = AddRun(current);

        // Act
        var result = await validator.ValidateAsync(run, requested, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe("Recalculation is not possible as there are no completed Initial runs for '2024'.");
    }

    [TestMethod]
    public async Task ValidateAsync_WhenTheYearHasACompletedInitialRun_AllowsARecalculation()
    {
        // Arrange
        AddRun(RunClassification.InitialCompleted, fssSentAt: FssSentAt);
        var run = AddRun(RunClassification.Unclassified, createdAt: FssSentAt.AddDays(1));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenCompletingAnInitialRun_ReturnsValid()
    {
        // Arrange
        var run = AddRun(RunClassification.Initial);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.InitialCompleted, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenCompletingARecalculationRun_ReturnsValid()
    {
        // Arrange
        AddRun(RunClassification.InitialCompleted, fssSentAt: FssSentAt);
        var run = AddRun(RunClassification.Recalculation, createdAt: FssSentAt.AddDays(1));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.RecalculationCompleted, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenRunWasCreatedBeforeTheBillingFileWasSentToFss_RejectsDesignation()
    {
        // Arrange
        AddRun(RunClassification.InitialCompleted, fssSentAt: FssSentAt);
        var run = AddRun(RunClassification.Unclassified, createdAt: FssSentAt.AddDays(-1));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe("This '2024' run cannot be classified as it is outdated.");
    }

    [TestMethod]
    public async Task ValidateAsync_WhenSeveralBillingFilesHaveBeenSentToFss_ComparesAgainstTheLatest()
    {
        // Arrange - the run predates only the most recent billing file
        AddRun(RunClassification.InitialCompleted, fssSentAt: FssSentAt);
        AddRun(RunClassification.RecalculationCompleted, fssSentAt: FssSentAt.AddDays(60));
        var run = AddRun(RunClassification.Unclassified, createdAt: FssSentAt.AddDays(30));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken);

        // Assert
        result.Errors.ShouldHaveSingleItem()
            .ShouldBe("This '2024' run cannot be classified as it is outdated.");
    }

    [TestMethod]
    public async Task ValidateAsync_WhenDesignatedRunsBelongToAnotherRelativeYear_IgnoresThem()
    {
        // Arrange
        AddRun(RunClassification.Initial, relativeYear: Year2025);
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_WhenOtherRunsInTheYearAreNotDesignated_IgnoresThem()
    {
        // Arrange
        AddRun(RunClassification.Running);
        AddRun(RunClassification.Errored);
        AddRun(RunClassification.Test);
        AddRun(RunClassification.Deleted);
        var run = AddRun(RunClassification.Unclassified);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken);

        // Assert
        result.Errors.ShouldBeEmpty();
    }

    /// <summary>
    ///     Adds a run to the database. Supplying <paramref name="fssSentAt"/> also attaches billing file
    ///     metadata authorised at that moment, which is what marks a completed run as sent to FSS.
    /// </summary>
    private CalculatorRun AddRun(
        RunClassification classification,
        RelativeYear? relativeYear = null,
        DateTime? createdAt = null,
        DateTime? fssSentAt = null)
    {
        var run = new CalculatorRun
        {
            Id = ++lastRunId,
            Name = $"Test Run {lastRunId}",
            Classification = classification,
            RelativeYear = relativeYear ?? Year2024,
            CreatedBy = "Test User",
            CreatedAt = createdAt ?? FssSentAt.AddDays(1)
        };

        dbContext.CalculatorRuns.Add(run);

        if (fssSentAt is not null)
        {
            dbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                CalculatorRunId = run.Id,
                BillingCsvFileName = $"billing-{run.Id}.csv",
                BillingJsonFileName = $"billing-{run.Id}.json",
                BillingFileCreatedBy = "Test User",
                BillingFileCreatedDate = fssSentAt.Value,
                BillingFileAuthorisedBy = "Test User",
                BillingFileAuthorisedDate = fssSentAt
            });
        }

        dbContext.SaveChanges();

        return run;
    }
}
