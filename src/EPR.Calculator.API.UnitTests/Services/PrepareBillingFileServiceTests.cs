using System.Net;
using System.Threading.Tasks;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class PrepareBillingFileServiceTests
    {
        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsUnprocessableContent_WhenRunNotFound()
        {
            // Arrange
            var dbContext = TestDbContextFactory.Create();
            var service = new PrepareBillingFileService(dbContext);

            // Act
            var result = await service.PrepareBillingFileAsync(999);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidRunId, result.Message);
        }

        [TestMethod]
        public async Task PrepareBillingFileAsync_ReturnsUnprocessableContent_WhenNoBillingInstructions()
        {
            // Arrange
            var dbContext = TestDbContextFactory.Create();
            dbContext.CalculatorRuns.Add(new CalculatorRun { Id = 1, CalculatorRunClassificationId = Util.AcceptableRunStatusForBillingInstructions().First(), Name = "Test", Financial_Year = new CalculatorRunFinancialYear { Name = "2025" }, CreatedBy = "user", CreatedAt = System.DateTime.Now });
            dbContext.SaveChanges();
            var service = new PrepareBillingFileService(dbContext);

            // Act
            var result = await service.PrepareBillingFileAsync(1);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableContent, result.StatusCode);
            Assert.AreEqual(ErrorMessages.InvalidOrganisationId, result.Message);
        }

        // Add more tests for the success path when you implement the file builder logic
    }

    // Helper factory for in-memory db context
    public static class TestDbContextFactory
    {
        public static ApplicationDBContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDb" + System.Guid.NewGuid())
                .Options;
            return new ApplicationDBContext(options);
        }
    }
}