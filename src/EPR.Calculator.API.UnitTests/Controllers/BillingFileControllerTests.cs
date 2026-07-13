using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class BillingFileControllerTests
{
    private const int RunId = 123;
    private const string CsvFilename = "billing.csv";

    private Mock<IBlobStorageService> blobStorageMock = null!;
    private ApplicationDBContext context = null!;
    private BillingFileController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        blobStorageMock = new Mock<IBlobStorageService>();

        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        context = new ApplicationDBContext(dbContextOptions);
        context.Database.EnsureCreated();

        controller = new BillingFileController(blobStorageMock.Object, context);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }

    [TestMethod]
    public async Task DownloadCsvBillingFile_ReturnsNotFound_WhenBillingMetadataDoesNotExist()
    {
        // Arrange (no billing metadata is seeded for the run)

        // Act
        var result = await controller.DownloadCsvBillingFile(RunId);

        // Assert
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadCsvBillingFile_ReturnsNotFound_WhenCsvFileMetadataDoesNotExist()
    {
        // Arrange
        SeedBillingMetadata(RunId, CsvFilename);

        // Act
        var result = await controller.DownloadCsvBillingFile(RunId);

        // Assert
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadCsvBillingFile_ReturnsNotFound_WhenBlobStreamIsNull()
    {
        // Arrange
        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await controller.DownloadCsvBillingFile(RunId);

        // Assert
        ShouldBeBillingFileNotFound(result, RunId);
    }

    [TestMethod]
    public async Task DownloadCsvBillingFile_ReturnsCsvFile_WhenBillingFileExists()
    {
        // Arrange
        SeedBillingMetadata(RunId, CsvFilename);
        SeedCsvMetadata(RunId, CsvFilename);
        using var stream = new MemoryStream();
        blobStorageMock
            .Setup(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        // Act
        var result = await controller.DownloadCsvBillingFile(RunId);

        // Assert
        var fileResult = result.ShouldBeOfType<FileStreamHttpResult>();
        fileResult.ContentType.ShouldBe("text/csv");
        fileResult.FileDownloadName.ShouldBe(CsvFilename);
        blobStorageMock.Verify(x => x.OpenBillingCsvStream(CsvFilename, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SeedBillingMetadata(int runId, string csvFilename)
    {
        context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runId,
            BillingCsvFileName = csvFilename,
            BillingFileCreatedBy = "test-user",
            BillingFileCreatedDate = DateTime.UtcNow,
            BillingJsonFileName = csvFilename[..^4] + ".json",
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

    private static void ShouldBeBillingFileNotFound(IResult result, int runId)
    {
        var notFound = result.ShouldBeOfType<NotFound<string>>();
        notFound.Value.ShouldBe(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));
    }
}
