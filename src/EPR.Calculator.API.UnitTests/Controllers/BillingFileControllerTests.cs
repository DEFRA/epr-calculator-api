using System.Text;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class BillingFileControllerTests
{
    private const int RunId = 123;
    private const string CsvFilename = "billing.csv";
    private const string JsonFilename = "billing.json";

    private Mock<IBlobStorageService> blobStorageMock = null!;
    private Mock<IFileExportService> fileExportServiceMock = null!;
    private ApplicationDBContext context = null!;
    private BillingFileController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        blobStorageMock = new Mock<IBlobStorageService>();
        fileExportServiceMock = new Mock<IFileExportService>();

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new ApplicationDBContext(dbContextOptions);
        context.Database.EnsureCreated();

        controller = new BillingFileController(blobStorageMock.Object, fileExportServiceMock.Object, context);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Exported_ReturnsFile()
    {
        var content = Encoding.UTF8.GetBytes("File");
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Exported(content, CsvFilename));

        var result = await controller.DownloadBillingCsv(RunId);

        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("text/csv");
        fileResult.FileDownloadName.ShouldBe(CsvFilename);
        fileResult.FileContents.ShouldBe(content);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_NotFound()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.NotFound());

        var result = await controller.DownloadBillingCsv(RunId);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task DownloadBillingJson_Exported_ReturnsFile()
    {
        var content = Encoding.UTF8.GetBytes("File");
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Exported(content, JsonFilename));

        var result = await controller.DownloadBillingJson(RunId);

        var fileResult = result.ShouldBeOfType<FileContentResult>();
        fileResult.ContentType.ShouldBe("application/json");
        fileResult.FileDownloadName.ShouldBe(JsonFilename);
        fileResult.FileContents.ShouldBe(content);
    }

    [TestMethod]
    public async Task DownloadBillingJson_NotFound()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.NotFound());

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenBillingMetadataDoesNotExist()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        var result = await controller.DownloadBillingCsv(RunId);

        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenCsvFileMetadataDoesNotExist()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        SeedBillingMetadata(RunId, CsvFilename);

        var result = await controller.DownloadBillingCsv(RunId);

        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenBlobStreamIsNull()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());
        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        var result = await controller.DownloadBillingCsv(RunId);

        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsCsvFile_WhenBillingFileExists()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Csv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingCsv(RunId);

        var fileResult = result.ShouldBeOfType<FileStreamResult>();
        fileResult.ContentType.ShouldBe("text/csv");
        fileResult.FileDownloadName.ShouldBe(CsvFilename);
        blobStorageMock.Verify(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsNotFound_WhenBillingMetadataDoesNotExist()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        var result = await controller.DownloadBillingJson(RunId);
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsNotFound_WhenBillingJsonFileNameIsEmpty()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());
            
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: string.Empty);
        var result = await controller.DownloadBillingJson(RunId);
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsNotFound_WhenBlobStreamIsNull()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());
            
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: JsonFilename);
        blobStorageMock
            .Setup(x => x.OpenBillingJsonStream(JsonFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        var result = await controller.DownloadBillingJson(RunId);
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsJsonFile_WhenBillingFileExists()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());
            
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: JsonFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingJsonStream(JsonFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingJson(RunId);

        var fileResult = result.ShouldBeOfType<FileStreamResult>();
        fileResult.ContentType.ShouldBe("application/json");
        fileResult.FileDownloadName.ShouldBe(JsonFilename);
        blobStorageMock.Verify(x => x.OpenBillingJsonStream(JsonFilename, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SeedBillingMetadata(int runId, string csvFilename, string? jsonFilename = null)
    {
        context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runId,
            BillingCsvFileName = csvFilename,
            BillingFileCreatedBy = "test-user",
            BillingFileCreatedDate = DateTime.UtcNow,
            BillingJsonFileName = jsonFilename ?? csvFilename[..^4] + ".json",
        });
        context.SaveChanges();
    }

    private void SeedCsvMetadata(int runId, string csvFilename)
    {
        context.CalculatorRunCsvFileMetadata.Add(new CalculatorRunCsvFileMetadata
        {
            CalculatorRunId = runId,
            FileName = csvFilename,
            BlobUri = $"https://example.com/{csvFilename}",
        });
        context.SaveChanges();
    }

    private static void ShouldBeBillingFileNotFound(IActionResult result, int runId)
    {
        var notFound = result.ShouldBeOfType<NotFoundObjectResult>();
        notFound.Value.ShouldBe(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));
    }
}
