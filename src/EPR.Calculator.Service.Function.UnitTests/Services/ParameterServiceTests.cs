using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ParameterService"/> class.
    /// </summary>
    [TestClass]
    public class ParameterServiceTests
    {
        private ApplicationDBContext dbContext;
        private ParameterService parameterService;

        public ParameterServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "ParameterTestDatabase")
                .Options;

            dbContext = new ApplicationDBContext(options);

            parameterService = new ParameterService(dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnDefaultParameters()
        {
            // Arrange
            var runContext = TestDataHelper.CalculatorRun2024;

            var run = new CalculatorRun
            {
                Id = runContext.RunId,
                Name = "Some name",
                DefaultParameterSettingMasterId = 100
            };

            var master = new DefaultParameterSettingMaster
            {
                Id = 100
            };

            var detail = new DefaultParameterSettingDetail
            {
                DefaultParameterSettingMasterId = 100,
                ParameterUniqueReferenceId = "PARAM1",
                ParameterValue = 123m
            };

            var template = new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "PARAM1",
                ParameterType = "Some Type",
                ParameterCategory = "Some Category"
            };

            dbContext.CalculatorRuns.Add(run);
            dbContext.DefaultParameterSettings.Add(master);
            dbContext.DefaultParameterSettingDetail.Add(detail);
            dbContext.DefaultParameterTemplateMasterList.Add(template);

            await dbContext.SaveChangesAsync();

            // Act
            var result = await parameterService.GetDefaultParameters(runContext);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(123m, result["PARAM1"]);
        }
    }
}
