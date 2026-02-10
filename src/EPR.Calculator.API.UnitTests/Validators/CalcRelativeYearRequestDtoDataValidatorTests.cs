using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Validators
{
    [TestClass]
    public class CalcRelativeYearRequestDtoDataValidatorTests
    {
        private readonly int calcRunId = 85885;
        private readonly int unclassifiedRunId = 85886;

        private ApplicationDBContext dbContext = null!;
        private CalcRelativeYearRequestDtoDataValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            // Use a unique DB name for each test run
            var dbName = $"TestDb_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            dbContext = new ApplicationDBContext(options);

            var calcRuns = new List<CalculatorRun>()
            {
                new()
                {
                    CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                    RelativeYear = new RelativeYear(2024),
                    Name = "Test",
                    Id = calcRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
                new()
                {
                    CalculatorRunClassificationId = (int)RunClassification.UNCLASSIFIED,
                    RelativeYear = new RelativeYear(2024),
                    Name = "Test",
                    Id = unclassifiedRunId,
                    CreatedBy = "Test",
                    CreatedAt = DateTime.UtcNow,
                },
            };
            dbContext.CalculatorRuns.AddRange(calcRuns);
            dbContext.CalculatorRunRelativeYears.Add(new CalculatorRunRelativeYear { Value = 2024 });
            dbContext.SaveChanges();
            validator = new CalcRelativeYearRequestDtoDataValidator(dbContext);
        }

        [TestMethod]
        [DataRow("1923-24")]
        public async Task Validate_ReturnsInvalid_WhenRelativeYearNotFoundInDatabase(string relativeYear)
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = calcRunId,
                RelativeYearValue = 2000,
            };

            // Act
            var result = await validator.Validate(request, CancellationToken.None);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Relative year not found in the database.");
        }

        [TestMethod]
        [DataRow(-1)]
        public async Task Validate_ReturnsInvalid_WhenRunNotFoundInDatabase(int runId)
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = runId,
                RelativeYearValue = 2024,
            };

            // Act
            var result = await validator.Validate(request, CancellationToken.None);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run not found in the database.");
        }

        [TestMethod]
        public async Task Validate_ReturnsInvalid_WhenRunIsAlreadyClassified()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = calcRunId,
                RelativeYearValue = 2024,
            };

            // Act
            var result = await validator.Validate(request, CancellationToken.None);

            // Assert
            result.IsInvalid.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Run is already classified.");
        }

        [TestMethod]
        public async Task Validate_ReturnsValid_WhenAllCriteriaMet()
        {
            // Arrange
            var request = new CalcRelativeYearRequestDto
            {
                RunId = unclassifiedRunId,
                RelativeYearValue = 2024,
            };

            // Act
            var result = await validator.Validate(request, CancellationToken.None);

            // Assert
            result.IsInvalid.Should().BeFalse();
        }
    }
}