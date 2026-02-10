using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class GetCalculatorRunTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IStorageService> mockStorageService;
        private readonly Mock<IServiceBusService> mockServiceBusService;
        private readonly Mock<ICalcRelativeYearRequestDtoDataValidator> mockValidator;

        public GetCalculatorRunTest()
        {
            this.mockStorageService = new Mock<IStorageService>();
            this.mockConfig = new Mock<IConfiguration>();
            this.mockServiceBusService = new Mock<IServiceBusService>();
            this.mockValidator = new Mock<ICalcRelativeYearRequestDtoDataValidator>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            this.RelativeYear24_25 = new CalculatorRunRelativeYear { Value = 2024 };
            this.context.CalculatorRunRelativeYears.Add(this.RelativeYear24_25);
            this.context.SaveChanges();
        }

        private CalculatorRunRelativeYear RelativeYear24_25 { get; init; }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task GetCalculatorRunTest_Get_Valid_Run()
        {
            var date = DateTime.UtcNow;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                RelativeYear = new RelativeYear(2024),
                IsBillingFileGenerating = true,
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>(),
                    Mock.Of<IBillingFileService>());

            var response = await controller.GetCalculatorRun(1, CancellationToken.None) as ObjectResult;
            Assert.IsNotNull(response);
            var run = response.Value as CalculatorRunDto;

            Assert.IsNotNull(run);

            Assert.AreEqual(1, run.RunId);
            Assert.AreEqual("RUNNING", run.RunClassificationStatus);
            Assert.AreEqual(date, run.CreatedAt);
            Assert.AreEqual(2, run.RunClassificationId);
            Assert.IsNull(run.UpdatedAt);
            Assert.IsNull(run.UpdatedBy);
            Assert.IsTrue(run.IsBillingFileGenerating);
        }

        [TestMethod]
        public async Task GetCalculatorRunTest_Get_Invalid_Run()
        {
            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>(),
                    Mock.Of<IBillingFileService>());

            var response = await controller.GetCalculatorRun(1, CancellationToken.None) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("Unable to find Run Id 1", response.Value);
        }
    }
}
