using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            this.dbContext = new ApplicationDBContext(options);

            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = this.financialYear };

            var calcRuns = new List<CalculatorRun>()
            {
                new()
                {
                    CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                    Financial_Year = calculatorRunFinancialYear,
                    Name = "Test",
                    Id = this.calcRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
                new CalculatorRun
                {
                    CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
                    Financial_Year = calculatorRunFinancialYear,
                    Name = "Test",
                    Id = this.unclassifiedRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
            };

            this.dbContext.CalculatorRuns.AddRange(calcRuns);
            this.dbContext.SaveChanges();
            this.validator = new CalcFinancialYearRequestDtoDataValidator(this.dbContext);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("\t")]
        public void Validate_ReturnsInvalid_WhenFinancialYearNullOrWhiteSpace(string financialYear)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = this.calcRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = this.validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Financial year is required.");
        }

        [TestMethod]
        [DataRow("1923-24")]
        public void Validate_ReturnsInvalid_WhenFinancialYearNotFoundInDatabase(string financialYear)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = this.calcRunId,
                FinancialYear = financialYear,
            };

            // Act
            var result = this.validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Financial year not found in the database.");
        }

        [TestMethod]
        [DataRow(-1)]
        public void Validate_ReturnsInvalid_WhenRunNotFoundInDatabase(int runId)
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = runId,
                FinancialYear = this.financialYear,
            };

            // Act
            var result = this.validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run not found in the database.");
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenRunIsAlreadyClassified()
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = this.calcRunId,
                FinancialYear = this.financialYear,
            };

            // Act
            var result = this.validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run is already classified.");
        }

        [TestMethod]
        public void Validate_ReturnsValid_WhenAllCriteriaMet()
        {
            // Arrange
            var request = new CalcFinancialYearRequestDto
            {
                RunId = this.unclassifiedRunId,
                FinancialYear = this.financialYear,
            };

            // Act
            var result = this.validator.Validate(request);

            // Assert
            result.IsInvalid.Should().BeFalse();
        }
    }
}