using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class DownloadResultFileTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusFactory;
        private readonly Mock<IStorageService> mockStorageService;

        public DownloadResultFileTest()
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
        public void DownloadResultFile_Test()
        {
            var date = new DateTime(2024, 11, 11);
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
            var mockResult = new Mock<IResult>();
            this.mockStorageService.Setup(x => x.DownloadFile(It.IsAny<string>())).ReturnsAsync(mockResult.Object);

            var downloadResultFile = controller.DownloadResultFile(1);

            downloadResultFile.Wait();

            var result1 = downloadResultFile.Result;

            this.mockStorageService.Verify(x => x.DownloadFile("1-Calc RunName_Results File_20241111.csv"));

            Assert.AreEqual(mockResult.Object, result1);
        }
    }
}
