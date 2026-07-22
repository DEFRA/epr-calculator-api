using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class DownloadResultFileTest
    {
        private const int RunId = 1;
        private const string ResultsFileName = "1-Calc RunName_Results File_20241111.csv";

        private ApplicationDBContext context = null!;
        private Mock<IBlobStorageService> mockBlobStorage = null!;
        private CalculatorController controller = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: $"PayCal-{Guid.NewGuid()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this.context = new ApplicationDBContext(options);
            this.context.Database.EnsureCreated();

            this.mockBlobStorage = new Mock<IBlobStorageService>();

            this.controller = new CalculatorController(
                this.context,
                this.mockBlobStorage.Object,
                Mock.Of<IServiceBusService>(),
                Mock.Of<ICalculatorRunStatusDataValidator>(),
                Mock.Of<ICalcRelativeYearRequestDtoDataValidator>(),
                Mock.Of<IAvailableClassificationsService>(),
                Mock.Of<ICalculationRunService>());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.context.Database.EnsureDeleted();
            this.context.Dispose();
        }

        [TestMethod]
        public async Task DownloadResultFile_ReturnsCsvFile_WhenMetadataAndBlobExist()
        {
            // Arrange
            this.AddResultsFileMetadata();
            using var stream = new MemoryStream();
            this.SetupResultCsvStream(stream);

            // Act
            var result = await this.controller.DownloadResultFile(RunId);

            // Assert
            var fileResult = result.ShouldBeOfType<FileStreamHttpResult>();
            fileResult.ContentType.ShouldBe("text/csv");
            fileResult.FileDownloadName.ShouldBe(ResultsFileName);
        }

        [TestMethod]
        public async Task DownloadResultFile_ReturnsNotFound_WhenMetadataMissing()
        {
            // Arrange - no CSV file metadata is seeded for the run.

            // Act
            var result = await this.controller.DownloadResultFile(RunId);

            // Assert
            var notFound = result.ShouldBeOfType<NotFound<string>>();
            notFound.Value.ShouldBe(string.Format(CommonResources.NoCSVFileFound, RunId));
        }

        [TestMethod]
        public async Task DownloadResultFile_ReturnsNotFound_WhenBlobStreamMissing()
        {
            // Arrange
            this.AddResultsFileMetadata();
            this.SetupResultCsvStream(null);

            // Act
            var result = await this.controller.DownloadResultFile(RunId);

            // Assert
            var notFound = result.ShouldBeOfType<NotFound<string>>();
            notFound.Value.ShouldBe(string.Format(CommonResources.NoCSVFileFound, RunId));
        }

        private void AddResultsFileMetadata()
        {
            this.context.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
            {
                CalculatorRunId = RunId,
                FileName = ResultsFileName,
                BlobUri = $"https://example.com/{ResultsFileName}",
            });
            this.context.SaveChanges();
        }

        private void SetupResultCsvStream(Stream? stream) =>
            this.mockBlobStorage
                .Setup(x => x.OpenResultCsvStream(ResultsFileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stream);
    }
}
