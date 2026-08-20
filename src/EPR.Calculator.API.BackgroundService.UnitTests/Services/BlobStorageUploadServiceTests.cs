using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Exceptions;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

[TestClass]
public class BlobStorageServiceTests
{
    private IFixture fixture = null!;
    private Mock<BlobClient> mockBlobClient = null!;
    private BlobStorageUploadService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();

        mockBlobClient = fixture.Freeze<Mock<BlobClient>>();

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        var mockBlobServiceClient = fixture.Freeze<Mock<BlobServiceClient>>();
        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockBlobContainerClient.Object);

        fixture.Inject(mockBlobServiceClient.Object);

        sut = fixture.Create<BlobStorageUploadService>();
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
    {
        // Arrange
        var request = new IStorageUploadService.Request
        {
            FileName = "test.txt",
            Content = "test content",
            ContainerName = fixture.Create<string>(),
        };

        var expectedUri = new Uri("https://example.com/test.txt");

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        mockBlobClient.Setup(x => x.Uri)
            .Returns(expectedUri);

        // Act
        var result = await sut.UploadFileContentAsync(request, CancellationToken.None);

        // Assert
        Assert.AreEqual(result, expectedUri.ToString());
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ShouldReturnFalse_WhenUploadFails()
    {
        // Arrange
        var request = new IStorageUploadService.Request
        {
            FileName = "test.txt",
            Content = "test content",
            ContainerName = fixture.Create<string>(),
        };

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestException());

        // Act & Assert
        await Should.ThrowAsync<TestException>(async () => await sut.UploadFileContentAsync(
            request, CancellationToken.None));
    }

    [TestMethod]
    public async Task UploadFileContentAsync_WritesUtf8Bom_WhenUseUtf8BomIsTrue()
    {
        // Arrange
        var request = new IStorageUploadService.Request
        {
            FileName = "test.txt",
            Content = "test content",
            ContainerName = fixture.Create<string>(),
            UseUtf8Bom = true
        };

        BinaryData? uploadedContent = null;

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<BinaryData, bool, CancellationToken>((data, _, _) => uploadedContent = data)
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        mockBlobClient.Setup(x => x.Uri)
            .Returns(new Uri("https://example.com/test.csv"));

        // Act
        await sut.UploadFileContentAsync(request, CancellationToken.None);

        // Assert
        var bytes = uploadedContent!.ToArray();
        bytes.Take(3).ShouldBe(new byte[] { 0xEF, 0xBB, 0xBF });
        bytes.Skip(3).ShouldBe(Encoding.UTF8.GetBytes("test content"));
    }

    [TestMethod]
    public async Task UploadFileContentAsync_DoesNotWriteUtf8Bom_WhenUseUtf8BomIsFalse()
    {
        // Arrange
        var request = new IStorageUploadService.Request
        {
            FileName = "test.txt",
            Content = "test content",
            ContainerName = fixture.Create<string>(),
            UseUtf8Bom = false
        };
        BinaryData? uploadedContent = null;

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<BinaryData, bool, CancellationToken>((data, _, _) => uploadedContent = data)
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        mockBlobClient.Setup(x => x.Uri)
            .Returns(new Uri("https://example.com/test.json"));

        // Act
        await sut.UploadFileContentAsync(request, CancellationToken.None);

        // Assert
        var bytes = uploadedContent!.ToArray();
        bytes.ShouldBe(Encoding.UTF8.GetBytes("test content"));
    }
}
