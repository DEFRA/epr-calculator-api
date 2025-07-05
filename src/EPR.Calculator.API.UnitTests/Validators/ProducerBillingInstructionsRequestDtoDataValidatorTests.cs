using System.Net;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Validators
{
    [TestClass]
    public class ProducerBillingInstructionsRequestDtoDataValidatorTests
    {
        private ApplicationDBContext dbContext = null!;
        private int calcRunId;

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
                HasBillingFileGenerated = true,
                Name = "Test",
                Id = this.calcRunId,
                CreatedBy = "Test",
                CreatedAt = System.DateTime.UtcNow,
            };

            this.dbContext.CalculatorRuns.Add(calcRun);
            this.dbContext.SaveChanges();
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenPageNumberNegative()
        {
            var dto = new ProducerBillingInstructionsRequestDto { PageNumber = -1 };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsTrue(result.IsInvalid);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenPageSizeLessThanOne()
        {
            var dto = new ProducerBillingInstructionsRequestDto { PageSize = 0 };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsTrue(result.IsInvalid);
            Assert.AreEqual("PageSize must be at least 1 if provided.", result.Errors.First().Message);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenOrganisationIdIsZero()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = 0 },
            };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsTrue(result.IsInvalid);
            Assert.AreEqual("OrganisationId must be greater than 0 if provided.", result.Errors.First().Message);
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenStatusHasInvalidValue()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { Status = new[] { "InvalidStatus" } },
            };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsTrue(result.IsInvalid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Status can only contain")));
        }

        [TestMethod]
        public void Validate_ReturnsInvalid_WhenStatusHasDuplicates()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { Status = new[] { "Accepted", "Accepted" } },
            };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsTrue(result.IsInvalid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("Status cannot contain duplicate values.")));
        }

        [TestMethod]
        public void Validate_ReturnsValid_WhenAllFieldsAreCorrect()
        {
            var dto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = 1,
                PageSize = 10,
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = 1, Status = new[] { "Accepted" } },
            };
            var validator = new ProducerBillingInstructionsRequestDtoDataValidator();
            var result = validator.Validate(dto);
            Assert.IsFalse(result.IsInvalid);
        }
    }
}