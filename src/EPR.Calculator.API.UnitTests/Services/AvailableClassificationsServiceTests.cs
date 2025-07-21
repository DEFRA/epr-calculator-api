using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services;
using EnumsNET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class AvailableClassificationsServiceTests
    {
        private ApplicationDBContext _dbContext = null!;
        private AvailableClassificationsService _service = null!;
        private Mock<ILogger<AvailableClassificationsService>> _loggerMock = null!;
        private const string FY = "2024-25";

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDBContext(options);

            // Add all possible classifications
            foreach (RunClassification value in Enum.GetValues(typeof(RunClassification)))
            {
                _dbContext.CalculatorRunClassifications.Add(new CalculatorRunClassification
                {
                    Id = (int)value,
                    Status = value.AsString(EnumFormat.Description)!
                });
            }

            _dbContext.FinancialYears.Add(new CalculatorRunFinancialYear
            {
                Name = FY
            });

            _dbContext.SaveChanges();

            _loggerMock = new Mock<ILogger<AvailableClassificationsService>>();
            _service = new AvailableClassificationsService(_dbContext, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Dispose();
        }

        private void AddRunToDb(RunClassification classification, int requestId)
        {
            _dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = requestId,
                CalculatorRunClassificationId = (int)classification,
                Name = $"Run{requestId}",
                FinancialYearId = FY,
                Financial_Year = _dbContext.FinancialYears.First(),
                CreatedBy = "TestUser",
                CreatedAt = DateTime.UtcNow,
            });
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public async Task Returns_InitialAndTestRun_WhenNoOtherRunsInYear()
        {
            // Arrange: no other runs for this year
            var request = new CalcFinancialYearRequestDto
            {
                RunId = 100,
                FinancialYear = FY
            };

            // Act
            var result = await _service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[]
                {
                    RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)!,
                    RunClassification.TEST_RUN.AsString(EnumFormat.Description)!
                },
                statuses);
        }

        [TestMethod]
        public async Task Returns_TestRun_WhenOnlyDesignatedButIncomplete()
        {
            // Arrange: Designated (e.g., INITIAL_RUN) but not Complete
            AddRunToDb(RunClassification.INITIAL_RUN, 10);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 99,
                FinancialYear = FY
            };

            // Act
            var result = await _service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var statuses = result.Select(c => c.Status).ToList();
            CollectionAssert.AreEquivalent(
                new[] { RunClassification.TEST_RUN.AsString(EnumFormat.Description)! },
                statuses);
        }

        [TestMethod]
        public async Task Returns_InterimFinalFinalRecalcTest_WhenHasInitialRunCompleted()
        {
            // Arrange: At least one INITIAL_RUN_COMPLETED, none FINAL_*_COMPLETED
            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, 11);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 21,
                FinancialYear = FY
            };

            // Act
            var result = await _service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var expected = new[]
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!
            };
            CollectionAssert.AreEquivalent(expected, result.Select(x => x.Status).ToList());
        }

        [TestMethod]
        public async Task Returns_InterimFinalTest_WhenHasFinalRecalculationButNoFinalRun()
        {
            // Arrange: INITIAL_RUN_COMPLETED and FINAL_RECALCULATION_RUN_COMPLETED, no FINAL_RUN_COMPLETED
            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, 12);
            AddRunToDb(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, 13);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 22,
                FinancialYear = FY
            };

            // Act
            var result = await _service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var expected = new[]
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!
            };
            CollectionAssert.AreEquivalent(expected, result.Select(x => x.Status).ToList());
        }

        [TestMethod]
        public async Task Returns_InterimTest_WhenHasFinalRun()
        {
            // Arrange: INITIAL_RUN_COMPLETED and FINAL_RUN_COMPLETED
            AddRunToDb(RunClassification.INITIAL_RUN_COMPLETED, 14);
            AddRunToDb(RunClassification.FINAL_RUN_COMPLETED, 15);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 23,
                FinancialYear = FY
            };

            // Act
            var result = await _service.GetAvailableClassificationsForFinancialYearAsync(request);

            // Assert
            var expected = new[]
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!
            };
            CollectionAssert.AreEquivalent(expected, result.Select(x => x.Status).ToList());
        }
        
        [TestMethod]
        public async Task ShouldLogErrorAndThrow_WhenDbThrows()
        {
            // Arrange: Setup broken context
            var brokenContext = new Mock<ApplicationDBContext>();
            brokenContext.Setup(x => x.CalculatorRuns).Throws(new Exception("DB fail"));
            var service = new AvailableClassificationsService(brokenContext.Object, _loggerMock.Object);

            var request = new CalcFinancialYearRequestDto
            {
                RunId = 25,
                FinancialYear = FY
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await service.GetAvailableClassificationsForFinancialYearAsync(request);
            });
        }
    }
}