using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Validators
{
    [TestClass]
    public class ProducerBillingInstructionsRequestDtoDataValidatorTests
    {
        private ApplicationDBContext dbContext = null!;
        private int calcRunId;
        private ProducerBillingInstructionsRequestDtoValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            // Use a unique DB name for each test run
            var dbName = $"TestDb_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            this.dbContext = new ApplicationDBContext(options);

            this.calcRunId = 85885;
            var calcRun = new CalculatorRun
            {
                CalculatorRunClassificationId = 8,
                Financial_Year = new CalculatorRunFinancialYear { Name = "2024-25" },
                Name = "Test",
                Id = this.calcRunId,
                CreatedBy = "Test",
                CreatedAt = System.DateTime.UtcNow,
            };

            this.dbContext.CalculatorRuns.Add(calcRun);
            this.dbContext.SaveChanges();
            this.validator = new ProducerBillingInstructionsRequestDtoValidator();
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenPageNumberNegative()
        {
            var dto = new ProducerBillingInstructionsRequestDto { PageNumber = -1 };
            var result = this.validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("PageNumber must be 1 or greater.");
        }

        [TestMethod]
        public void Should_HaveValidationError_When_PageSize_IsLessThan1()
        {
            var dto = new ProducerBillingInstructionsRequestDto { PageSize = 0 };
            var result = this.validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("PageSize must be at least 1 if provided.");
        }

        [TestMethod]
        public void Should_HaveValidationError_When_OrganisationId_IsZero()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = 0 },
            };
            var result = this.validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor("SearchQuery.OrganisationId")
                .WithErrorMessage("OrganisationId must be greater than 0 if provided.");
        }

        [TestMethod]
        public void Should_HaveValidationError_When_Status_HasInvalidValue()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { Status = new[] { "InvalidStatus" } },
            };
            var result = this.validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor("SearchQuery.Status")
                .WithErrorMessage("Status can only contain: Accepted, Rejected, Pending.");
        }

        [TestMethod]
        public void Should_HaveValidationError_When_Status_HasDuplicates()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { Status = new[] { "Accepted", "Accepted" } },
            };
            var result = this.validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor("SearchQuery.Status")
                .WithErrorMessage("Status cannot contain duplicate values.");
        }

        [TestMethod]
        public void Should_NotHaveValidationError_When_AllFieldsAreValid()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = 1, Status = new[] { "Accepted" } },
            };
            var result = this.validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}