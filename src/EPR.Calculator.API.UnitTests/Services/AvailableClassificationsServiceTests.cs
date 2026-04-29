using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="AvailableClassificationsService"/>.
    /// Note:  is safe here because all enum values are decorated with Description.
    /// </summary>
    [TestClass]
    public class AvailableClassificationsServiceTests
    {
        // Fields, no underscores per SonarQube
        private ApplicationDBContext dbContext = null!;
        private AvailableClassificationsService service = null!;

        public TestContext TestContext { get; set; }

        /// <summary>
        /// Sets up a fresh in-memory context and service for each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new ApplicationDBContext(options);

            // Add dummy RelativeYear for navigation property
            dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear
            {
                Value = 2024,
            });

            dbContext.SaveChanges();

            service = new AvailableClassificationsService(dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ReturnsInitialAndTestRun_WhenNoOtherRunsInYear()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 1,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.InitialRun,
                    RunClassification.TestRun
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsTestRun_WhenOnlyDesignatedNotComplete()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 99,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.InitialRun, runId: 10, isComplete: false);
            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.TestRun,
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsInterimFinalFinalRecalcTest_WhenHasInitialRunCompleted()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 999,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.InitialRunCompleted, runId: 10, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.InterimRecalculationRun,
                    RunClassification.FinalRecalculationRun,
                    RunClassification.FinalRun,
                    RunClassification.TestRun
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsTestRun_WhenHasInitialRunCompletedAndCurrentUnclassifiedRunIsOlder()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 999,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);
            AddRunToDb(RunClassification.InitialRunCompleted, runId: 10, isComplete: true);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.TestRun,
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsTestRun_WhenHasCompletedRunsAndCurrentUnclassifiedRunIsOlder()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 999,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);
            AddRunToDb(RunClassification.InitialRunCompleted, runId: 10, isComplete: true);
            AddRunToDb(RunClassification.InterimRecalculationRun, runId: 11, isComplete: true);
            AddRunToDb(RunClassification.FinalRecalculationRunCompleted, runId: 12, isComplete: true);
            AddRunToDb(RunClassification.FinalRunCompleted, runId: 13, isComplete: true);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.TestRun
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsInterimFinalTest_WhenHasFinalRecalcButNoFinalRun()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 888,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.InitialRunCompleted, runId: 10, isComplete: true);
            AddRunToDb(RunClassification.FinalRecalculationRunCompleted, runId: 11, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.InterimRecalculationRun,
                    RunClassification.FinalRun,
                    RunClassification.TestRun
                },
                result);
        }

        [TestMethod]
        public async Task ReturnsInterimTest_WhenHasFinalRun()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = 777,
                RelativeYearValue = 2024,
            };

            AddRunToDb(RunClassification.InitialRunCompleted, runId: 10, isComplete: true);
            AddRunToDb(RunClassification.FinalRunCompleted, runId: 11, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.Unclassified, runId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.InterimRecalculationRun,
                    RunClassification.TestRun,
                },
                result);
        }

        /// <summary>
        /// Helper method to add a CalculatorRun to the database.
        /// </summary>
        /// <param name="classification">Classification enum value.</param>
        /// <param name="runId">Run Id.</param>
        /// <param name="isComplete">If run is completed.</param>
        private void AddRunToDb(RunClassification classification, int runId, bool isComplete)
        {
            var currentTime = DateTime.UtcNow;
            string userName = "TestUser";

            var run = new CalculatorRun
            {
                Id = runId,
                Classification = classification,
                Name = "Test",
                RelativeYear = new RelativeYear(2024),
                CreatedBy = userName,
                CreatedAt = currentTime,
            };

            if (isComplete)
            {
                run.BillingRunStatus = BillingRunStatus.Completed;
                run.BillingFileMetadata = new CalculatorRunBillingFileMetadata
                {
                    CalculatorRunId = runId,
                    BillingFileCreatedBy = userName,
                    BillingFileCreatedDate = currentTime.AddMicroseconds(1),
                    BillingFileAuthorisedBy = userName,
                    BillingFileAuthorisedDate = currentTime.AddMicroseconds(2), // ensure it's after CreatedAt
                    BillingCsvFileName = "ignored",
                    BillingJsonFileName = "ignored"
                };
            }

            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();
        }
    }
}
