using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ClassificationService"/> class.
    /// </summary>
    [TestClass]
    public class ClassificationServiceTests
    {
        private Mock<IDbContextFactory<ApplicationDBContext>> dbContextFactory;
        private ApplicationDBContext dbContext;
        private ClassificationService classificationService;

        public ClassificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            dbContextFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
            dbContext = new ApplicationDBContext(options);
            dbContextFactory.Setup(factory => factory.CreateDbContext()).Returns(dbContext);
            classificationService = new ClassificationService(dbContextFactory.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        [DataRow(1, RunClassification.RUNNING, 2023)]
        [DataRow(2, RunClassification.UNCLASSIFIED, 2024)]
        [DataRow(3, RunClassification.ERROR, 2025)]
        public async Task ShouldUpdateRunClassification(int runId, RunClassification runClassification, int relativeYearValue)
        {
            // Arrange
            dbContext.CalculatorRuns.RemoveRange(dbContext.CalculatorRuns);
            await dbContext.SaveChangesAsync();
            dbContext.CalculatorRuns.Add(new CalculatorRun { Id = runId, Name = "Test Run 01", RelativeYear = new RelativeYear(relativeYearValue) });
            await dbContext.SaveChangesAsync();

            // Act
            await classificationService.UpdateRunClassification(runId, runClassification);

            // Assert
            var calculatorRun = await dbContext.CalculatorRuns.FirstOrDefaultAsync(run => run.Id == runId);
            Assert.AreEqual(calculatorRun?.CalculatorRunClassificationId, (int)runClassification);
        }

        [TestMethod]
        public async Task ShouldReturnKeyNotFoundException()
        {
            // Arrange
            var runId = 10;

            // Act
            var exceptionResult = await Should.ThrowAsync<KeyNotFoundException>(() => classificationService.UpdateRunClassification(runId, RunClassification.ERROR));

            // Assert
            Assert.AreEqual("Calculator run id 10 not found", exceptionResult.Message);
        }
    }
}