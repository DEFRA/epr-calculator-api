using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Validators;

[TestClass]
public class RunClassificationValidatorTests
{
    private static readonly RelativeYear Year = new(2024);

    private ApplicationDBContext dbContext = null!;
    private RunClassificationValidator validator = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new ApplicationDBContext(options);
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = Year });
        dbContext.SaveChanges();

        validator = new RunClassificationValidator(dbContext);
    }

    [TestCleanup]
    public void TearDown()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [TestMethod]
    [DataRow(RunClassification.None)]
    [DataRow(RunClassification.Unknown)]
    public async Task ValidateAsync_ReturnsInvalid_WhenClassificationIsNotAssignable(RunClassification classification)
    {
        // Arrange
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, classification, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"The classification '{classification}' is not assignable.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenRunIsAlreadyCompleted()
    {
        // Arrange: once a run's billing file has been shared, it cannot be reclassified - not even to Deleted.
        // The DeleteCalculatorRun endpoint relies solely on this check to stop completed runs being deleted.
        var run = CreateRun(1, RunClassification.Initial, Year, isBillingFileShared: true);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe("Cannot reclassify a run once the run is completed.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsValid_WhenClassifyingAsDeleted_EvenWhenYearHasOutstandingRuns()
    {
        // Arrange: Deleted is decided before any year-level conflict checks, so outstanding runs elsewhere shouldn't matter.
        dbContext.CalculatorRuns.Add(CreateRun(2, RunClassification.Initial, Year));
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.Test, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Deleted, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsValid_WhenClassifyingAsTest_EvenWhenYearHasOutstandingRuns()
    {
        // Arrange: Test runs are not "Official", so they're exempt from the year-level conflict rules below.
        dbContext.CalculatorRuns.Add(CreateRun(2, RunClassification.Initial, Year));
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Test, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
    }

    [TestMethod]
    public async Task ValidateAsync_ExcludesTheRunBeingValidated_FromItsOwnConflictCheck()
    {
        // Arrange: the run being reclassified is itself an incomplete Initial run in the database. If it weren't
        // excluded from the year query, it would be flagged as an "outstanding" conflict against itself.
        var run = CreateRun(1, RunClassification.Initial, Year);
        dbContext.CalculatorRuns.Add(run);
        dbContext.SaveChanges();

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenYearHasIncompleteInitialRun_AndDesignatingRecalculation()
    {
        // Arrange
        dbContext.CalculatorRuns.Add(CreateRun(2, RunClassification.Initial, Year));
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"There are outstanding runs for '{Year}' that have not been completed.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenYearHasIncompleteRecalculationRun_AndDesignatingInitial()
    {
        // Arrange: an incomplete run of *either* Official classification blocks a new Official designation.
        dbContext.CalculatorRuns.Add(CreateRun(2, RunClassification.Recalculation, Year));
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"There are outstanding runs for '{Year}' that have not been completed.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenInitialAlreadyCompletedForYear()
    {
        // Arrange
        var completedInitial = CreateRun(2, RunClassification.Initial, Year, isBillingFileShared: true);
        completedInitial.CalculatorRunBillingFileMetadata.Add(CreateSharedBillingFileMetadata(DateTime.UtcNow.AddDays(-1)));
        dbContext.CalculatorRuns.Add(completedInitial);
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"A completed Initial run already exists for '{Year}'.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenRecalculationRequested_ButYearHasNoCompletedInitialRun()
    {
        // Arrange: no runs exist for the year at all.
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"Recalculation is not possible as there are no completed Initial runs for '{Year}'.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsInvalid_WhenRunPredatesTheLatestSharedBillingFileForTheYear()
    {
        // Arrange: an Official run for the year already had its billing file authorised *after* this run was
        // created, so this (now out-of-sequence) run can no longer be classified as Official.
        var authorisedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var completedInitial = CreateRun(2, RunClassification.Initial, Year, isBillingFileShared: true);
        completedInitial.CalculatorRunBillingFileMetadata.Add(CreateSharedBillingFileMetadata(authorisedAt));
        dbContext.CalculatorRuns.Add(completedInitial);
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year, createdAt: authorisedAt.AddDays(-1));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().ShouldBe($"This '{Year}' run cannot be classified as it is outdated.");
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsValid_WhenDesignatingInitial_AndYearHasNoOtherOfficialRuns()
    {
        // Arrange
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_ReturnsValid_WhenDesignatingRecalculation_AfterInitialWasCompletedEarlier()
    {
        // Arrange
        var authorisedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var completedInitial = CreateRun(2, RunClassification.Initial, Year, isBillingFileShared: true);
        completedInitial.CalculatorRunBillingFileMetadata.Add(CreateSharedBillingFileMetadata(authorisedAt));
        dbContext.CalculatorRuns.Add(completedInitial);
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year, createdAt: authorisedAt.AddDays(1));

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Recalculation, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_IgnoresCompletedRuns_FromOtherRelativeYears()
    {
        // Arrange: a completed Initial run exists, but for a different year - it must not affect this year's checks.
        var otherYear = new RelativeYear(Year.Value - 1);
        dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = otherYear });
        var completedInitialOtherYear = CreateRun(2, RunClassification.Initial, otherYear, isBillingFileShared: true);
        completedInitialOtherYear.CalculatorRunBillingFileMetadata.Add(CreateSharedBillingFileMetadata(DateTime.UtcNow));
        dbContext.CalculatorRuns.Add(completedInitialOtherYear);
        dbContext.SaveChanges();
        var run = CreateRun(1, RunClassification.None, Year);

        // Act
        var result = await validator.ValidateAsync(run, RunClassification.Initial, CancellationToken.None);

        // Assert
        result.IsInvalid.ShouldBeFalse();
    }

    private static CalculatorRun CreateRun(
        int id,
        RunClassification classification,
        RelativeYear relativeYear,
        bool isBillingFileShared = false,
        DateTime? createdAt = null) => new()
    {
        Id = id,
        Name = $"Run {id}",
        Classification = classification,
        RelativeYear = relativeYear,
        IsBillingFileShared = isBillingFileShared,
        CreatedBy = "Test",
        CreatedAt = createdAt ?? DateTime.UtcNow,
    };

    private static CalculatorRunBillingFileMetadata CreateSharedBillingFileMetadata(DateTime authorisedDate) => new()
    {
        BillingCsvFileName = "billing.csv",
        BillingJsonFileName = "billing.json",
        BillingFileCreatedDate = authorisedDate,
        BillingFileCreatedBy = "Test",
        BillingFileAuthorisedDate = authorisedDate,
        BillingFileAuthorisedBy = "Test",
    };
}
