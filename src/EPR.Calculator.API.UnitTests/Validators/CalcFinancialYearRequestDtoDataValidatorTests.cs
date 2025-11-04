using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Validators
{
    [TestClass]
    public class CalcFinancialYearRequestDtoDataValidatorTests
    {
        private readonly int calcRunId = 85885;
        private readonly int unclassifiedRunId = 85886;
        private readonly string financialYear = "2024-25";

        private ApplicationDBContext dbContext = null!;
        private CalcFinancialYearRequestDtoDataValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            // Use a unique DB name for each test run
            var dbName = $"TestDb_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            dbContext = new ApplicationDBContext(options);

            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = financialYear };

            var calcRuns = new List<CalculatorRun>()
            {
                new()
                {
                    CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                    Financial_Year = calculatorRunFinancialYear,
                    Name = "Test",
                    Id = calcRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
                new()
                {
                    CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
                    Financial_Year = calculatorRunFinancialYear,
                    Name = "Test",
                    Id = unclassifiedRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
            };

            dbContext.CalculatorRuns.AddRange(calcRuns);
            dbContext.SaveChanges();
            validator = new CalcFinancialYearRequestDtoDataValidator(dbContext);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("\t")]
        public async Task Validate_ReturnsInvalid_WhenFinancialYearNullOrWhiteSpace(string financialYear)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = calcRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = await validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Financial year is required.");
        }

        [TestMethod]
        [DataRow("1923-24")]
        public async Task Validate_ReturnsInvalid_WhenFinancialYearNotFoundInDatabase(string financialYear)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = calcRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = await validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Financial year not found in the database.");
        }

        [TestMethod]
        [DataRow(-1)]
        public async Task Validate_ReturnsInvalid_WhenRunNotFoundInDatabase(int runId)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = runId,
                FinancialYear = financialYear,
            };

            // Act
            var result = await validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run not found in the database.");
        }

        [TestMethod]
        public async Task Validate_ReturnsInvalid_WhenRunIsAlreadyClassified()
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = calcRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = await validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run is already classified.");
        }

        [TestMethod]
        public async Task Validate_ReturnsValid_WhenAllCriteriaMet()
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = unclassifiedRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = await validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeFalse();
        }
    }
}