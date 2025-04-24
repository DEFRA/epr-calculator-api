using AutoFixture;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class DownloadBillingFileTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IStorageService> mockStorageService;
        private readonly Mock<IServiceBusService> mockServiceBusService;

        public DownloadBillingFileTest()
        {
            this.Fixture = new Fixture();

            this.mockStorageService = new Mock<IStorageService>();
            this.mockServiceBusService = new Mock<IServiceBusService>();
            this.mockConfig = new Mock<IConfiguration>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            this.FinancialYear24_25 = new CalculatorRunFinancialYear { Name = "2024-25" };
            this.context.FinancialYears.Add(this.FinancialYear24_25);
            this.context.SaveChanges();
        }

        private Fixture Fixture { get; init; }

        private CalculatorRunFinancialYear FinancialYear24_25 { get; init; }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ShouldReturnFileResult_WhenFileExists()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var fileName = this.Fixture.Create<string>();
            var blobUri = this.Fixture.Create<string>();
            var runName = this.Fixture.Create<string>();

            var calculatorRun = new CalculatorRun
            {
                Id = runId,
                Name = runName,
                CalculatorRunClassificationId = this.Fixture.Create<int>(),
                CreatedAt = this.Fixture.Create<DateTime>(),
                CreatedBy = this.Fixture.Create<string>(),
                LapcapDataMasterId = this.Fixture.Create<int>(),
                DefaultParameterSettingMasterId = this.Fixture.Create<int>(),
                Financial_Year = FinancialYear24_25
            };

            this.context.CalculatorRuns.Add(calculatorRun);

            this.context.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
            {
                CalculatorRunId = runId,
                FileName = fileName,
                BlobUri = blobUri,
                CalculatorRun = calculatorRun,
            });

            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object);
            var mockResult = new Mock<IResult>();

            this.mockStorageService.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mockResult.Object);

            // Act
            var downloadBillingFile = await controller.DownloadBillingFile(runId);

            // Assert
            this.mockStorageService.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            Assert.AreEqual(mockResult.Object, downloadBillingFile);
        }

        [TestMethod]
        public async Task DownloadBilling_ShouldReturnNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var runId = this.Fixture.Create<int>();
            var fileName = this.Fixture.Create<string>();
            var blobUri = this.Fixture.Create<string>();
            var downloadFileName = this.Fixture.Create<string>();

            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                Name = this.Fixture.Create<string>(),
                CalculatorRunClassificationId = this.Fixture.Create<int>(),
                CreatedAt = this.Fixture.Create<DateTime>(),
                CreatedBy = this.Fixture.Create<string>(),
                LapcapDataMasterId = this.Fixture.Create<int>(),
                DefaultParameterSettingMasterId = this.Fixture.Create<int>(),
                Financial_Year = FinancialYear24_25,
            });

            this.context.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
            {
                CalculatorRunId = runId,
                FileName = fileName,
                BlobUri = blobUri,
            });

            this.context.SaveChanges();

            var controller =
                new CalculatorController(
                    this.context,
                    this.mockConfig.Object,
                    this.mockStorageService.Object,
                    this.mockServiceBusService.Object);

            this.mockStorageService.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Results.NotFound(fileName));

            // Act
            var downloadBillingFile = await controller.DownloadBillingFile(runId);

            // Assert
            this.mockStorageService.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            Assert.IsInstanceOfType(downloadBillingFile, typeof(NotFound<string>));
            var notFoundObjectResult = (NotFound<string>)downloadBillingFile;
            Assert.AreEqual(fileName, notFoundObjectResult.Value);
        }
    }
}