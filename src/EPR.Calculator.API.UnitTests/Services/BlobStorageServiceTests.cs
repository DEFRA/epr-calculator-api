using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.UnitTests.Services;

[TestClass]
public class BlobStorageServiceTests
{
    private const string ResultCsvContainerName = "result-csv";
    private const string BillingCsvContainerName = "billing-csv";
    private const string BillingJsonContainerName = "billing-json";
    private const string FssContainerName = "fss";
    private const string TestFilename = "test-file.csv";
    private Mock<BlobContainerClient> billingCsvContainer = null!;
    private Mock<BlobContainerClient> billingJsonContainer = null!;

    private Mock<BlobServiceClient> blobServiceClient = null!;
    private Mock<BlobContainerClient> fssContainer = null!;
    private Mock<BlobContainerClient> resultCsvContainer = null!;
    private BlobStorageService service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        resultCsvContainer = new Mock<BlobContainerClient>();
        billingCsvContainer = new Mock<BlobContainerClient>();
        billingJsonContainer = new Mock<BlobContainerClient>();
        fssContainer = new Mock<BlobContainerClient>();
        billingJsonContainer.SetupGet(x => x.Name).Returns(BillingJsonContainerName);
        fssContainer.SetupGet(x => x.Name).Returns(FssContainerName);

        blobServiceClient = new Mock<BlobServiceClient>();
        blobServiceClient.Setup(x => x.GetBlobContainerClient(ResultCsvContainerName)).Returns(resultCsvContainer.Object);
        blobServiceClient.Setup(x => x.GetBlobContainerClient(BillingCsvContainerName)).Returns(billingCsvContainer.Object);
        blobServiceClient.Setup(x => x.GetBlobContainerClient(BillingJsonContainerName)).Returns(billingJsonContainer.Object);
        blobServiceClient.Setup(x => x.GetBlobContainerClient(FssContainerName)).Returns(fssContainer.Object);

        var options = Microsoft.Extensions.Options.Options.Create(new BlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ResultFileCsvContainer = ResultCsvContainerName,
            BillingFileCsvContainer = BillingCsvContainerName,
            BillingFileJsonContainer = BillingJsonContainerName,
            FssContainer = FssContainerName
        });

        service = new BlobStorageService(blobServiceClient.Object, options);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsStreamPositionedAtStart()
    {
        // Arrange
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, "position check"u8.ToArray());

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Position.ShouldBe(0L);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Constructor_CreatesContainerClientForEachConfiguredContainer()
    {
        blobServiceClient.Verify(x => x.GetBlobContainerClient(ResultCsvContainerName), Times.Once);
        blobServiceClient.Verify(x => x.GetBlobContainerClient(BillingCsvContainerName), Times.Once);
        blobServiceClient.Verify(x => x.GetBlobContainerClient(BillingJsonContainerName), Times.Once);
        blobServiceClient.Verify(x => x.GetBlobContainerClient(FssContainerName), Times.Once);
    }

    // ── OpenResultCsvStream ───────────────────────────────────────────────────

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsReEncodedContent_WhenBlobExists()
    {
        // Arrange
        const string content = "col1,col2\nval1,val2";
        SetupBlobClientWithContent(resultCsvContainer, TestFilename, content);

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        (await ReadContentAsync(result)).ShouldBe(content);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsNull_WhenBlobNotFound()
    {
        // Arrange
        SetupBlobClientWith404(resultCsvContainer, TestFilename);

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    // ── OpenBillingCsvStream ──────────────────────────────────────────────────

    [TestMethod]
    public async Task OpenBillingCsvStream_ReturnsReEncodedContent_WhenBlobExists()
    {
        // Arrange
        const string content = "billing,data\n1,2";
        SetupBlobClientWithContent(billingCsvContainer, TestFilename, content);

        // Act
        var result = await service.OpenBillingCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        (await ReadContentAsync(result)).ShouldBe(content);
    }

    [TestMethod]
    public async Task OpenBillingCsvStream_ReturnsNull_WhenBlobNotFound()
    {
        // Arrange
        SetupBlobClientWith404(billingCsvContainer, TestFilename);

        // Act
        var result = await service.OpenBillingCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    // ── OpenBillingJsonStream ─────────────────────────────────────────────────

    [TestMethod]
    public async Task OpenBillingJsonStream_ReturnsReEncodedContent_WhenBlobExists()
    {
        // Arrange
        const string content = "{\"billing\":true}";
        SetupBlobClientWithContent(billingJsonContainer, TestFilename, content);

        // Act
        var result = await service.OpenBillingJsonStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        (await ReadContentAsync(result)).ShouldBe(content);
    }

    [TestMethod]
    public async Task OpenBillingJsonStream_FallsBackToFss_WhenBlobNotFoundInBillingJsonContainer()
    {
        // Arrange
        SetupBlobClientWith404(billingJsonContainer, TestFilename);
        const string content = "{\"fromFss\":true}";
        SetupBlobClientWithContent(fssContainer, TestFilename, content);

        // Act
        var result = await service.OpenBillingJsonStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        (await ReadContentAsync(result)).ShouldBe(content);
    }

    [TestMethod]
    public async Task OpenBillingJsonStream_ReturnsNull_WhenBlobNotFoundInEitherContainer()
    {
        // Arrange
        SetupBlobClientWith404(billingJsonContainer, TestFilename);
        SetupBlobClientWith404(fssContainer, TestFilename);

        // Act
        var result = await service.OpenBillingJsonStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void SetupBlobClientWithContent(Mock<BlobContainerClient> container, string filename, string utf8Content)
        => SetupBlobClientWithBytes(container, filename, Encoding.UTF8.GetBytes(utf8Content));

    private static void SetupBlobClientWithBytes(Mock<BlobContainerClient> container, string filename, byte[] bytes)
    {
        var blobStream = new MemoryStream(bytes);
        var blobClient = new Mock<BlobClient>();
        blobClient.Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobStream);
        container.Setup(x => x.GetBlobClient(filename)).Returns(blobClient.Object);
    }

    private static void SetupBlobClientWith404(Mock<BlobContainerClient> container, string filename)
    {
        var blobClient = new Mock<BlobClient>();
        blobClient.Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(status: 404, "BlobNotFound"));
        container.Setup(x => x.GetBlobClient(filename)).Returns(blobClient.Object);
    }

    private static async Task<string> ReadContentAsync(Stream stream)
    {
        // detectEncodingFromByteOrderMarks reads the UTF-16 LE BOM written by ReEncodeAsUtf16Async
        // and decodes the stream correctly, returning the original string content.
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }
}