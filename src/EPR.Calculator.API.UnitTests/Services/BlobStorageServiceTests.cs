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
    private static readonly UnicodeEncoding Utf16 = new(bigEndian: false, byteOrderMark: true);
    private Mock<BlobContainerClient> billingCsvContainer = null!;
    private Mock<BlobContainerClient> billingJsonContainer = null!;

    private Mock<BlobServiceClient> blobServiceClient = null!;
    private Mock<BlobContainerClient> fssContainer = null!;
    private Mock<ILogger<BlobStorageService>> logger = null!;
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

        logger = new Mock<ILogger<BlobStorageService>>();

        var options = Microsoft.Extensions.Options.Options.Create(new BlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ResultFileCsvContainer = ResultCsvContainerName,
            BillingFileCsvContainer = BillingCsvContainerName,
            BillingFileJsonContainer = BillingJsonContainerName,
            FssContainer = FssContainerName
        });

        service = new BlobStorageService(blobServiceClient.Object, options, logger.Object);
    }

    // ── Re-encoding behaviour (via OpenResultCsvStream) ─────────────────────

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsUtf16WithBom_WhenBlobIsUtf8()
    {
        // Arrange
        const string content = "hello, world";
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, Encoding.UTF8.GetBytes(content));

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        AssertIsUtf16WithBom(await ReadBytesAsync(result), content);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsUtf16WithBom_WhenBlobIsUtf8WithBom()
    {
        // Arrange
        const string content = "test content with UTF-8 BOM";
        var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bytes = utf8Bom.GetPreamble().Concat(utf8Bom.GetBytes(content)).ToArray();
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, bytes);

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        AssertIsUtf16WithBom(await ReadBytesAsync(result), content);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_IsIdempotent_WhenBlobIsAlreadyUtf16()
    {
        // Arrange
        const string content = "already UTF-16";
        var bytes = Utf16.GetPreamble().Concat(Utf16.GetBytes(content)).ToArray();
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, bytes);

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        AssertIsUtf16WithBom(await ReadBytesAsync(result), content);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsUtf16WithBom_WhenBlobIsEmpty()
    {
        // Arrange
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, []);

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        AssertIsUtf16WithBom(await ReadBytesAsync(result), string.Empty);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_PreservesMultibyteUnicodeCharacters()
    {
        // Arrange
        const string content = "H\u00e9llo W\u00f6rld \U0001f30d";
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, Encoding.UTF8.GetBytes(content));

        // Act
        var result = await service.OpenResultCsvStream(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        AssertIsUtf16WithBom(await ReadBytesAsync(result), content);
    }

    [TestMethod]
    public async Task OpenResultCsvStream_ReturnsStreamPositionedAtStart()
    {
        // Arrange
        SetupBlobClientWithBytes(resultCsvContainer, TestFilename, Encoding.UTF8.GetBytes("position check"));

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

    // ── MoveBillingJsonToFss ──────────────────────────────────────────────────

    [TestMethod]
    public async Task MoveBillingJsonToFss_ReturnsFalse_WhenSourceBlobDoesNotExist()
    {
        // Arrange
        var sourceBlobClient = new Mock<BlobClient>();
        sourceBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(value: false, Mock.Of<Response>()));
        billingJsonContainer.Setup(x => x.GetBlobClient(TestFilename)).Returns(sourceBlobClient.Object);

        // Act
        var result = await service.MoveBillingJsonToFss(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public async Task MoveBillingJsonToFss_ReturnsTrue_WhenCopyAndDeleteSucceed()
    {
        // Arrange
        var sourceBlobClient = SetupCopyScenario(throwOnCopy: false);
        SetupDeleteReturns(sourceBlobClient, succeeds: true);

        // Act
        var result = await service.MoveBillingJsonToFss(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public async Task MoveBillingJsonToFss_ReturnsFalse_WhenCopyThrowsRequestFailedException()
    {
        // Arrange
        SetupCopyScenario(throwOnCopy: true);

        // Act
        var result = await service.MoveBillingJsonToFss(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
        VerifyLoggedOnce(LogLevel.Error);
    }

    [TestMethod]
    public async Task MoveBillingJsonToFss_ReturnsTrue_WhenCopySucceedsButDeleteFails()
    {
        // Arrange
        var sourceBlobClient = SetupCopyScenario(throwOnCopy: false);
        SetupDeleteReturns(sourceBlobClient, succeeds: false);

        // Act
        var result = await service.MoveBillingJsonToFss(TestFilename, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        VerifyLoggedOnce(LogLevel.Warning);
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

    private Mock<BlobClient> SetupCopyScenario(bool throwOnCopy)
    {
        var sourceBlobClient = new Mock<BlobClient>();
        sourceBlobClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(value: true, Mock.Of<Response>()));
        sourceBlobClient.SetupGet(x => x.Uri)
            .Returns(new Uri("https://test.blob.core.windows.net/billing-json/test-file.csv"));
        billingJsonContainer.Setup(x => x.GetBlobClient(TestFilename)).Returns(sourceBlobClient.Object);

        fssContainer.Setup(x => x.CreateIfNotExistsAsync(
                It.IsAny<PublicAccessType>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<BlobContainerEncryptionScopeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        var destBlobClient = new Mock<BlobClient>();
        fssContainer.Setup(x => x.GetBlobClient(TestFilename)).Returns(destBlobClient.Object);

        if (throwOnCopy)
        {
            destBlobClient.Setup(x => x.StartCopyFromUriAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<RehydratePriority?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(status: 500, "InternalServerError"));
        }
        else
        {
            var copyOp = new Mock<CopyFromUriOperation>();
            copyOp.Setup(x => x.WaitForCompletionAsync(It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult(Response.FromValue(value: 0L, Mock.Of<Response>())));
            destBlobClient.Setup(x => x.StartCopyFromUriAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<AccessTier?>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<RehydratePriority?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(copyOp.Object);
        }

        return sourceBlobClient;
    }

    private static void SetupDeleteReturns(Mock<BlobClient> sourceBlobClient, bool succeeds)
    {
        sourceBlobClient.Setup(x => x.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(succeeds, Mock.Of<Response>()));
    }

    private void VerifyLoggedOnce(LogLevel level) =>
        logger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

    private static async Task<byte[]> ReadBytesAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static async Task<string> ReadContentAsync(Stream stream)
    {
        // detectEncodingFromByteOrderMarks reads the UTF-16 LE BOM written by ReEncodeAsUtf16Async
        // and decodes the stream correctly, returning the original string content.
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    private static void AssertIsUtf16WithBom(byte[] bytes, string expectedContent)
    {
        var bom = Utf16.GetPreamble();
        bytes.Length.ShouldBeGreaterThanOrEqualTo(bom.Length);
        bytes[..bom.Length].ShouldBe(bom, "Expected UTF-16 LE byte-order mark");

        var decoded = Utf16.GetString(bytes, bom.Length, bytes.Length - bom.Length);
        decoded.ShouldBe(expectedContent);
    }
}
