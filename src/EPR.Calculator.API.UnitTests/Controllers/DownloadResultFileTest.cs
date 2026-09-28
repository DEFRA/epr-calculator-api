using System.Text;
using EPR.Calculator.API.BackgroundService;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class DownloadResultCsvTest
    {
        private const int RunId = 1;
        private const string ResultsFileName = "1-Calc RunName_Results File_20241111.csv";

        private ApplicationDBContext context = null!;
        private Mock<IBlobStorageService> mockBlobStorage = null!;
        private Mock<IFileExportService> fileExportServiceMock = null!;
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
            this.fileExportServiceMock = new Mock<IFileExportService>();

            this.controller = new CalculatorController(
                this.context,
                this.mockBlobStorage.Object,
                Mock.Of<IBackgroundTaskQueue>(),
                Mock.Of<ICalculatorRunStatusDataValidator>(),
                Mock.Of<ICalcRelativeYearRequestDtoDataValidator>(),
                Mock.Of<IAvailableClassificationsService>(),
                Mock.Of<ICalculationRunService>(),
                fileExportServiceMock.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.context.Database.EnsureDeleted();
            this.context.Dispose();
        }

        [TestMethod]
        public async Task DownloadResultCsv_Exported_ReturnsFile()
        {
            var content = Encoding.UTF8.GetBytes("File");
            var fileName = "test.csv";
            fileExportServiceMock
                .Setup(x => x.Export(It.IsAny<int>(), RunType.Calculator, FileExportType.Csv, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Exported(content, fileName));

            var result = await controller.DownloadResultCsv(RunId);

            var fileResult = result.ShouldBeOfType<FileContentResult>();
            fileResult.ContentType.ShouldBe("text/csv");
            fileResult.FileDownloadName.ShouldBe(fileName);
            fileResult.FileContents.ShouldBe(content);
        }

        [TestMethod]
        public async Task DownloadResultCsv_NotFound()
        {
            fileExportServiceMock
                .Setup(x => x.Export(It.IsAny<int>(), RunType.Calculator, FileExportType.Csv, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.NotFound());

            var result = await controller.DownloadResultCsv(RunId);

            result.ShouldBeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task DownloadResultCsv_Legacy_ReturnsCsvFile_WhenMetadataAndBlobExist()
        {
            fileExportServiceMock
                .Setup(x => x.Export(RunId, RunType.Calculator, FileExportType.Csv, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Legacy());
                
            this.AddResultsFileMetadata();
            using var stream = new MemoryStream();
            this.SetupResultCsvStream(stream);

            // Act
            var result = await this.controller.DownloadResultCsv(RunId);

            // Assert
            var fileResult = result.ShouldBeOfType<FileStreamResult>();
            fileResult.ContentType.ShouldBe("text/csv");
            fileResult.FileDownloadName.ShouldBe(ResultsFileName);
        }

        [TestMethod]
        public async Task DownloadResultCsv_Legacy_ReturnsNotFound_WhenMetadataMissing()
        {
            fileExportServiceMock
                .Setup(x => x.Export(RunId, RunType.Calculator, FileExportType.Csv, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Legacy());

            // Arrange - no CSV file metadata is seeded for the run.

            // Act
            var result = await this.controller.DownloadResultCsv(RunId);

            // Assert
            var notFound = result.ShouldBeOfType<NotFoundObjectResult>();
            notFound.Value.ShouldBe(string.Format(CommonResources.NoCSVFileFound, RunId));
        }

        [TestMethod]
        public async Task DownloadResultCsv_Legacy_ReturnsNotFound_WhenBlobStreamMissing()
        {
            fileExportServiceMock
                .Setup(x => x.Export(RunId, RunType.Calculator, FileExportType.Csv, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Legacy());

            // Arrange
            this.AddResultsFileMetadata();
            this.SetupResultCsvStream(null);

            // Act
            var result = await this.controller.DownloadResultCsv(RunId);

            // Assert
            var notFound = result.ShouldBeOfType<NotFoundObjectResult>();
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
