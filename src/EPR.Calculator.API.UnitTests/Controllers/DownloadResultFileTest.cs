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

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class DownloadResultFileTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusFactory;
        private readonly Mock<IStorageService> mockStorageService;
        private readonly Mock<IServiceBusService> mockServiceBusService;

        public DownloadResultFileTest()
        {
            mockStorageService = new Mock<IStorageService>();
            mockServiceBusService = new Mock<IServiceBusService>();
            mockConfig = new Mock<IConfiguration>();
            mockServiceBusFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();
        }

        [TestCleanup]
        public void CleanUp()
        {
            context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void DownloadResultFile_Test()
        {
            var date = new DateTime(2024, 11, 11);
            context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = "2024-25"
            });
            context.SaveChanges();

            var controller =
                new CalculatorController(context, mockConfig.Object, mockServiceBusFactory.Object,
                    mockStorageService.Object, mockServiceBusService.Object);
            var mockResult = new Mock<IResult>();
            mockStorageService.Setup(x => x.DownloadFile(It.IsAny<string>())).ReturnsAsync(mockResult.Object);

            var downloadResultFile = controller.DownloadResultFile(1);

            downloadResultFile.Wait();

            var result1 = downloadResultFile.Result;

            mockStorageService.Verify(x => x.DownloadFile("1-Calc RunName_Results File_20241111.csv"));

            Assert.AreEqual(mockResult.Object, result1);
        }
    }
}
