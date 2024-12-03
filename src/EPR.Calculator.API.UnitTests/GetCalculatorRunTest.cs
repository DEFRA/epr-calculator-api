using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class GetCalculatorRunTest
    {
        private ApplicationDBContext context;
        private Mock<IConfiguration> mockConfig;
        private Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusFactory;
        private Mock<IStorageService> mockStorageService;

        [TestInitialize]
        public void SetUp()
        {
            this.mockStorageService = new Mock<IStorageService>();
            this.mockConfig = new Mock<IConfiguration>();
            this.mockServiceBusFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void GetCalculatorRunTest_Get_Valid_Run()
        {
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = "2024-25"
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object,
                    this.mockStorageService.Object);

            var response = controller.GetCalculatorRun(1) as ObjectResult;
            Assert.IsNotNull(response);
            var run = response.Value as CalculatorRunDto;

            Assert.IsNotNull(run);

            Assert.AreEqual(1, run.RunId);
            Assert.AreEqual("RUNNING", run.RunClassificationStatus);
            Assert.AreEqual(date, run.CreatedAt);
            Assert.AreEqual(2, run.RunClassificationId);
            Assert.IsNull(run.UpdatedAt);
            Assert.IsNull(run.UpdatedBy);
        }

        [TestMethod]
        public void GetCalculatorRunTest_Get_Invalid_Run()
        {
            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object,
                    this.mockStorageService.Object);

            var response = controller.GetCalculatorRun(1) as ObjectResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("Unable to find Run Id 1", response.Value);
        }
    }
}
