using System.Security.Claims;
using System.Text;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Http;
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
        context.CalculatorRuns.Add(new CalculatorRun { Id = RunId, Name = "Run", RelativeYear = new RelativeYear(2025) });
        context.SaveChanges();

        controller = CreateController(downloadFromBlobStorage: false);
        SignInAs("SASuperUser");
    }

    private BillingFileController CreateController(bool downloadFromBlobStorage) =>
        new(
            blobStorageMock.Object,
            fileExportServiceMock.Object,
            context,
            Microsoft.Extensions.Options.Options.Create(new EPR.Calculator.API.Options.FeatureFlagOptions { DownloadFromBlobStorage = downloadFromBlobStorage }));

    private void SignInAs(string role) =>
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test")) }
        };

    private async Task MarkBillingFileShared()
    {
        context.CalculatorRuns.Single(r => r.Id == RunId).IsBillingFileShared = true;
        await context.SaveChangesAsync();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Streamed_ReturnsStreamedFile()
    {
        var content = Encoding.UTF8.GetBytes("File");
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((stream, ct) => stream.WriteAsync(content, ct).AsTask(), CsvFilename));

        var result = await controller.DownloadBillingCsv(RunId);

        var fileResult = result.ShouldBeOfType<PushStreamFileResult>();
        fileResult.ContentType.ShouldBe("text/csv");
        fileResult.FileDownloadName.ShouldBe(CsvFilename);
        using var written = new MemoryStream();
        await fileResult.WriteTo(written, CancellationToken.None);
        written.ToArray().ShouldBe(content);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_NotFound()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.NotFound());

        var result = await controller.DownloadBillingCsv(RunId);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task DownloadBillingJson_Streamed_ReturnsStreamedFile()
    {
        var content = Encoding.UTF8.GetBytes("File");
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((stream, ct) => stream.WriteAsync(content, ct).AsTask(), JsonFilename));

        var result = await controller.DownloadBillingJson(RunId);

        var fileResult = result.ShouldBeOfType<PushStreamFileResult>();
        fileResult.ContentType.ShouldBe("application/json");
        fileResult.FileDownloadName.ShouldBe(JsonFilename);
        using var written = new MemoryStream();
        await fileResult.WriteTo(written, CancellationToken.None);
        written.ToArray().ShouldBe(content);
    }

    [TestMethod]
    public async Task DownloadBillingJson_ForFss_ReturnsNotFound_WhenTheBillingFileHasNotBeenShared()
    {
        SignInAs("FssApiPolicy");

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<NotFoundObjectResult>().Value.ShouldBe($"Billing file for run {RunId} has not been shared.");
        fileExportServiceMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task DownloadBillingJson_ForFss_ReturnsNotFound_WhenTheRunDoesNotExist()
    {
        SignInAs("FssApiPolicy");

        var result = await controller.DownloadBillingJson(RunId + 1);

        result.ShouldBeOfType<NotFoundObjectResult>();
        fileExportServiceMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task DownloadBillingJson_ForFss_ReturnsTheFile_OnceTheBillingFileHasBeenShared()
    {
        SignInAs("FssApiPolicy");
        await MarkBillingFileShared();
        fileExportServiceMock
            .Setup(x => x.Export(RunId, FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((_, _) => Task.CompletedTask, JsonFilename));

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<PushStreamFileResult>();
    }

    [TestMethod]
    public async Task DownloadBillingJson_ForASuperUser_ReturnsTheFile_BeforeTheBillingFileHasBeenShared()
    {
        fileExportServiceMock
            .Setup(x => x.Export(RunId, FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((_, _) => Task.CompletedTask, JsonFilename));

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<PushStreamFileResult>();
    }

    [TestMethod]
    public async Task DownloadBillingJson_NotFound()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.NotFound());

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenBillingMetadataDoesNotExist()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        var result = await controller.DownloadBillingCsv(RunId);

        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenCsvFileMetadataDoesNotExist()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        SeedBillingMetadata(RunId, CsvFilename);

        var result = await controller.DownloadBillingCsv(RunId);

        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_Legacy_ReturnsNotFound_WhenBlobStreamIsNull()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());

        var result = await controller.DownloadBillingJson(RunId);
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsNotFound_WhenBillingJsonFileNameIsEmpty()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Legacy());
            
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: string.Empty);
        var result = await controller.DownloadBillingJson(RunId);
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadBillingJson_Legacy_ReturnsNotFound_WhenBlobStreamIsNull()
    {
        fileExportServiceMock
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
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
            .Setup(x => x.Export(It.IsAny<int>(), FileExportType.BillingJson, It.IsAny<CancellationToken>()))
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

    [TestMethod]
    public async Task DownloadBillingCsv_DownloadFromBlobStorage_ReturnsTheBlobFile_WithoutBuildingItFromTheDatabase()
    {
        controller = CreateController(downloadFromBlobStorage: true);
        SignInAs("SASuperUser");
        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingCsv(RunId);

        var fileResult = result.ShouldBeOfType<FileStreamResult>();
        fileResult.FileDownloadName.ShouldBe(CsvFilename);
        fileExportServiceMock.Verify(x => x.Export(It.IsAny<int>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingJson_DownloadFromBlobStorage_ReturnsTheBlobFile_WithoutBuildingItFromTheDatabase()
    {
        controller = CreateController(downloadFromBlobStorage: true);
        SignInAs("SASuperUser");
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: JsonFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingJsonStream(JsonFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingJson(RunId);

        var fileResult = result.ShouldBeOfType<FileStreamResult>();
        fileResult.FileDownloadName.ShouldBe(JsonFilename);
        fileExportServiceMock.Verify(x => x.Export(It.IsAny<int>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingJson_DownloadFromBlobStorage_ForFss_StillReturnsNotFound_WhenTheBillingFileHasNotBeenShared()
    {
        controller = CreateController(downloadFromBlobStorage: true);
        SignInAs("FssApiPolicy");
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: JsonFilename);

        var result = await controller.DownloadBillingJson(RunId);

        result.ShouldBeOfType<NotFoundObjectResult>().Value.ShouldBe(string.Format(CommonResources.BillingFileNotShared, RunId));
        blobStorageMock.Verify(x => x.OpenBillingJsonStream(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_QueryParamFalse_BuildsFromTheDatabase_EvenWhenTheFeatureFlagIsOn()
    {
        controller = CreateController(downloadFromBlobStorage: true);
        SignInAs("SASuperUser");
        fileExportServiceMock
            .Setup(x => x.Export(RunId, FileExportType.BillingCsv, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((_, _) => Task.CompletedTask, CsvFilename));

        var result = await controller.DownloadBillingCsv(RunId, downloadFromBlobStorage: false);

        result.ShouldBeOfType<PushStreamFileResult>();
        blobStorageMock.Verify(x => x.OpenBillingCsvStream(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingCsv_QueryParamTrue_ServesFromBlobStorage_EvenWhenTheFeatureFlagIsOff()
    {
        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingCsv(RunId, downloadFromBlobStorage: true);

        result.ShouldBeOfType<FileStreamResult>().FileDownloadName.ShouldBe(CsvFilename);
        fileExportServiceMock.Verify(x => x.Export(It.IsAny<int>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingJson_QueryParamFalse_BuildsFromTheDatabase_EvenWhenTheFeatureFlagIsOn()
    {
        controller = CreateController(downloadFromBlobStorage: true);
        SignInAs("SASuperUser");
        fileExportServiceMock
            .Setup(x => x.Export(RunId, FileExportType.BillingJson, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileExportResult.Streamed((_, _) => Task.CompletedTask, JsonFilename));

        var result = await controller.DownloadBillingJson(RunId, downloadFromBlobStorage: false);

        result.ShouldBeOfType<PushStreamFileResult>();
        blobStorageMock.Verify(x => x.OpenBillingJsonStream(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingJson_QueryParamTrue_ServesFromBlobStorage_EvenWhenTheFeatureFlagIsOff()
    {
        SeedBillingMetadata(RunId, CsvFilename, jsonFilename: JsonFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingJsonStream(JsonFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var result = await controller.DownloadBillingJson(RunId, downloadFromBlobStorage: true);

        result.ShouldBeOfType<FileStreamResult>().FileDownloadName.ShouldBe(JsonFilename);
        fileExportServiceMock.Verify(x => x.Export(It.IsAny<int>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()), Times.Never);
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
