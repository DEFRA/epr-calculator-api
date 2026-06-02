using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="AvailableClassificationsService"/>.
    /// Note: .ToString()! is safe here because all enum values are decorated with Description.
    /// </summary>
    [TestClass]
    public class AvailableClassificationsServiceTests
    {
        // Fields, no underscores per SonarQube
        private ApplicationDBContext dbContext = null!;
        private AvailableClassificationsService service = null!;
        private Mock<ILogger<AvailableClassificationsService>> loggerMock = null!;

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

            // Add all possible classifications
            foreach (RunClassification value in Enum.GetValues(typeof(RunClassification)))
            {
                dbContext.CalculatorRunClassifications.Add(new CalculatorRunClassification
                {
                    Id = (int)value,
                    Status = value.ToString()
                });
            }

            // Add dummy RelativeYear for navigation property
            dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear
            {
                Value = new RelativeYear(2024)
            });

            dbContext.SaveChanges();

            loggerMock = new Mock<ILogger<AvailableClassificationsService>>();
            service = new AvailableClassificationsService(dbContext, loggerMock.Object);
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

            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.INITIAL_RUN),
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.INITIAL_RUN, requestId: 10, isComplete: false);
            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.INTERIM_RECALCULATION_RUN),
                    nameof(RunClassification.FINAL_RECALCULATION_RUN),
                    nameof(RunClassification.FINAL_RUN),
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);
            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);
            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            AddRunToDb(RunClassification.INTERIM_RECALCULATION_RUN, requestId: 11, isComplete: true);
            AddRunToDb(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, requestId: 12, isComplete: true);
            AddRunToDb(RunClassification.FINAL_RUN_COMPLETED, requestId: 13, isComplete: true);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            AddRunToDb(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, requestId: 11, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.INTERIM_RECALCULATION_RUN),
                    nameof(RunClassification.FINAL_RUN),
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
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

            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            AddRunToDb(RunClassification.FINAL_RUN_COMPLETED, requestId: 11, isComplete: true);

            await Task.Delay(3, TestContext.CancellationTokenSource.Token); // ensure different CreatedAt timestamps
            AddRunToDb(RunClassification.UNCLASSIFIED, requestId: request.RunId, isComplete: false);

            // Act
            var result = await service.GetAvailableClassificationsForRelativeYearAsync(request, TestContext.CancellationTokenSource.Token);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof(RunClassification.INTERIM_RECALCULATION_RUN),
                    nameof(RunClassification.TEST_RUN)
                },
                statuses);
        }

        /// <summary>
        /// Helper method to add a CalculatorRun to the database.
        /// </summary>
        /// <param name="classification">Classification enum value.</param>
        /// <param name="requestId">Run Id.</param>
        /// <param name="isComplete">If run is completed.</param>
        private void AddRunToDb(RunClassification classification, int requestId, bool isComplete)
        {
            var currentTime = DateTime.UtcNow;
            string userName = "TestUser";

            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = requestId,
                CalculatorRunClassificationId = (int)classification,
                Name = "Test",
                RelativeYear = new RelativeYear(2024),
                CreatedBy = userName,
                CreatedAt = currentTime,
            });

            if (isComplete)
            {
                dbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
                {
                    CalculatorRunId = requestId,
                    BillingJsonFileName = "ignored",
                    BillingCsvFileName = "ignored",
                    BillingFileCreatedBy = userName,
                    BillingFileCreatedDate = currentTime.AddMicroseconds(1),
                    BillingFileAuthorisedBy = userName,
                    BillingFileAuthorisedDate = currentTime.AddMicroseconds(2), // ensure it's after CreatedAt
                });
            }

            dbContext.SaveChanges();
        }
    }
}
