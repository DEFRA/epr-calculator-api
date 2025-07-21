using EnumsNET;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="AvailableClassificationsService"/>.
    /// Note: .AsString(EnumFormat.Description)! is safe here because all enum values are decorated with Description.
    /// </summary>
    [TestClass]
    public class AvailableClassificationsServiceTests
    {
        // Constants first, per SonarQube
        private const string FinancialYear = "2024-25";

        // Fields, no underscores per SonarQube
        private ApplicationDBContext dbContext = null!;
        private AvailableClassificationsService service = null!;
        private Mock<ILogger<AvailableClassificationsService>> loggerMock = null!;

        /// <summary>
        /// Sets up a fresh in-memory context and service for each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            this.dbContext = new ApplicationDBContext(options);

            // Add all possible classifications
            foreach (RunClassification value in Enum.GetValues(typeof(RunClassification)))
            {
                this.dbContext.CalculatorRunClassifications.Add(new CalculatorRunClassification
                {
                    Id = (int)value,
                    Status = value.AsString(EnumFormat.Description)!, // not null by contract
                });
            }

            // Add dummy FinancialYear for navigation property
            this.dbContext.FinancialYears.Add(new CalculatorRunFinancialYear
            {
                Name = FinancialYear,
            });

            this.dbContext.SaveChanges();

            this.loggerMock = new Mock<ILogger<AvailableClassificationsService>>();
            this.service = new AvailableClassificationsService(this.dbContext, this.loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.dbContext.Dispose();
        }

        [TestMethod]
        public async Task ReturnsInitialAndTestRun_WhenNoOtherRunsInYear()
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = 1,
                FinancialYear = FinancialYear,
            };

            // Act
            var result = await this.service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
                },
                statuses);
        }

        [TestMethod]
        public async Task ReturnsTestRun_WhenOnlyDesignatedNotComplete()
        {
            // Arrange
            this.AddRunToDb(RunClassification.INITIAL_RUN, requestId: 10, isComplete: false);
            var request = new CalcFinancialYearRequestDto
            {
                RunId = 99,
                FinancialYear = FinancialYear,
            };

            // Act
            var result = await this.service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
                },
                statuses);
        }

        [TestMethod]
        public async Task ReturnsInterimFinalFinalRecalcTest_WhenHasInitialRunCompleted()
        {
            // Arrange
            this.AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            var request = new CalcFinancialYearRequestDto
            {
                RunId = 999,
                FinancialYear = FinancialYear,
            };

            // Act
            var result = await this.service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.FINAL_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
                },
                statuses);
        }

        [TestMethod]
        public async Task ReturnsInterimFinalTest_WhenHasFinalRecalcButNoFinalRun()
        {
            // Arrange
            this.AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            this.AddRunToDb(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, requestId: 11, isComplete: true);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 888,
                FinancialYear = FinancialYear,
            };

            // Act
            var result = await this.service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
                },
                statuses);
        }

        [TestMethod]
        public async Task ReturnsInterimTest_WhenHasFinalRun()
        {
            // Arrange
            this.AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, requestId: 10, isComplete: true);
            this.AddRunToDb(RunClassification.FINAL_RUN_COMPLETED, requestId: 11, isComplete: true);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 777,
                FinancialYear = FinancialYear,
            };

            // Act
            var result = await this.service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
                },
                statuses);
        }

        [TestMethod]
        public async Task ShouldLogErrorAndThrow_WhenDbThrows()
        {
            // Arrange: setup broken context
            var brokenContext = new Mock<ApplicationDBContext>();
            brokenContext.Setup(x => x.CalculatorRuns).Throws(new Exception("DB fail"));
            var serviceLocal = new AvailableClassificationsService(brokenContext.Object, this.loggerMock.Object);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 1,
                FinancialYear = FinancialYear,
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await serviceLocal.GetAvailableClassificationsForFinancialYearAsync(request);
            });
        }

        /// <summary>
        /// Helper method to add a CalculatorRun to the database.
        /// </summary>
        /// <param name="classification">Classification enum value</param>
        /// <param name="requestId">Run Id</param>
        /// <param name="isComplete">If run is completed</param>
        private void AddRunToDb(RunClassification classification, int requestId, bool isComplete)
        {
            this.dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = requestId,
                CalculatorRunClassificationId = (int)classification,
                Name = "Test",
                FinancialYearId = FinancialYear,
                Financial_Year = this.dbContext.FinancialYears.First(),
                CreatedBy = "TestUser",
                CreatedAt = DateTime.UtcNow,
            });
            this.dbContext.SaveChanges();
        }
    }
}