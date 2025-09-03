using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
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
    public class DownloadResultFileTest
    {
        private readonly ApplicationDBContext context;
        private readonly Mock<IConfiguration> mockConfig;
        private readonly Mock<IStorageService> mockStorageService;
        private readonly Mock<IServiceBusService> mockServiceBusService;
        private readonly Mock<ICalcFinancialYearRequestDtoDataValidator> mockValidator;

        public DownloadResultFileTest()
        {
            this.mockStorageService = new Mock<IStorageService>();
            this.mockServiceBusService = new Mock<IServiceBusService>();
            this.mockValidator = new Mock<ICalcFinancialYearRequestDtoDataValidator>();
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

        private CalculatorRunFinancialYear FinancialYear24_25 { get; init; }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task DownloadResultFile_ShouldReturnFileResult_WhenFileExists()
        {
            // Arrange
            var date = new DateTime(2024, 11, 11, 0, 0, 0, DateTimeKind.Unspecified);
            var runId = 1;
            var fileName = "1-Calc RunName_Results File_20241111.csv";
            var blobUri = $"https://example.com/{fileName}";

            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = FinancialYear24_25
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
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>());
            var mockResult = new Mock<IResult>();
            this.mockStorageService.Setup(x => x.DownloadFile(fileName, blobUri)).ReturnsAsync(mockResult.Object);

            // Act
            var downloadResultFile = await controller.DownloadResultFile(runId);

            // Assert
            this.mockStorageService.Verify(x => x.DownloadFile(fileName, blobUri));
            Assert.AreEqual(mockResult.Object, downloadResultFile);
        }

        [TestMethod]
        public async Task DownloadResultFile_ShouldReturnNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var runId = 1;
            var fileName = "1-Calc RunName_Results File_20241111.csv";
            var blobUri = $"https://example.com/{fileName}";

            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = new DateTime(2024, 11, 11, 0, 0, 0, DateTimeKind.Unspecified),
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
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
                    this.mockServiceBusService.Object,
                    this.mockValidator.Object,
                    Mock.Of<IAvailableClassificationsService>(),
                    Mock.Of<ICalculationRunService>());
            var mockResult = new Mock<IResult>();
            this.mockStorageService.Setup(x => x.DownloadFile(fileName, blobUri)).ReturnsAsync(Results.NotFound(fileName));

            // Act
            var downloadResultFile = await controller.DownloadResultFile(runId);

            // Assert
            this.mockStorageService.Verify(x => x.DownloadFile(fileName, blobUri));
            Assert.IsInstanceOfType(downloadResultFile, typeof(NotFound<string>));
            var notFoundObjectResult = (NotFound<string>)downloadResultFile;
            Assert.AreEqual(fileName, notFoundObjectResult.Value);
        }
    }
}