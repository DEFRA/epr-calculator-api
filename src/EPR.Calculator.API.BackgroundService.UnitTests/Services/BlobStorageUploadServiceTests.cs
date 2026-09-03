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
    private Mock<BlobContainerClient> mockBlobContainerClient = null!;
    private BlobStorageUploadService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();

        mockBlobClient = fixture.Freeze<Mock<BlobClient>>();

        mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobContainerClient.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(value: true, Mock.Of<Response>()));
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
            Content = Encoding.UTF8.GetBytes("test content"),
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
            Content = Encoding.UTF8.GetBytes("test content"),
            ContainerName = fixture.Create<string>(),
        };

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestException());

        // Act & Assert
        await Should.ThrowAsync<TestException>(async () => await sut.UploadFileContentAsync(
            request, CancellationToken.None));
    }

    [TestMethod]
    public async Task UploadFileContentAsync_UploadsContentBytesUnmodified()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("test content");
        var request = new IStorageUploadService.Request
        {
            FileName = "test.txt",
            Content = content,
            ContainerName = fixture.Create<string>(),
        };

        BinaryData? uploadedContent = null;

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback<BinaryData, bool, CancellationToken>((data, _, _) => uploadedContent = data)
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        mockBlobClient.Setup(x => x.Uri)
            .Returns(new Uri("https://example.com/test.txt"));

        // Act
        await sut.UploadFileContentAsync(request, CancellationToken.None);

        // Assert
        uploadedContent!.ToArray().ShouldBe(content);
    }
}
