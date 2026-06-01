using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="RunNameService"/> class.
    /// </summary>
    [TestClass]
    public class RunNameServiceTests
    {
        private ApplicationDBContext dbContext;
        private RunNameService runNameService;

        public RunNameServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            dbContext = new ApplicationDBContext(options);

            runNameService = new RunNameService(dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> returns the run name when the run exists.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldReturnRunNameWhenRunExists()
        {
            // Arrange
            var runId = 1;
            var expectedRunName = "Test Run Name";
            dbContext.CalculatorRuns.Add(new CalculatorRun { Id = runId, Name = expectedRunName, RelativeYear = new RelativeYear(2027) });
            await dbContext.SaveChangesAsync();

            // Act
            var result = await runNameService.GetRunNameAsync(runId);

            // Assert
            Assert.AreEqual(expectedRunName, result);
        }

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> returns exception when the run does not exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldReturnKeyNotFoundExceptionWhenRunDoesNotExist()
        {
            // Arrange
            var runId = 10;

            // Act
            var exceptionResult = await Should.ThrowAsync<KeyNotFoundException>(() => runNameService.GetRunNameAsync(runId));

            // Assert
            Assert.AreEqual("Calculator run with id 10 not found", exceptionResult.Message);
        }

        /// <summary>
        /// Tests that <see cref="RunNameService.GetRunNameAsync(int)"/> returns exception when the run name is null or empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TestMethod]
        public async Task GetRunNameAsync_ShouldReturnArgumentNullExceptionWhenRunNameIsNullOrEmpty()
        {
            // Arrange
            var runId = 2;
            dbContext.CalculatorRuns.Add(new CalculatorRun { Id = runId, Name = string.Empty, RelativeYear = new RelativeYear(2028) });
            await dbContext.SaveChangesAsync();

            // Act
            var exceptionResult = await Should.ThrowAsync<ArgumentNullException>(() => runNameService.GetRunNameAsync(runId));

            // Assert
            Assert.AreEqual("Value cannot be null. (Parameter 'Run name not found for the run id 2')", exceptionResult.Message);
        }
    }
}