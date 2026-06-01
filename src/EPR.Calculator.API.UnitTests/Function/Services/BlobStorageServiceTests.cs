using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Exceptions;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class BlobStorageServiceTests
{
    private IFixture fixture = null!;
    private Mock<BlobClient> mockBlobClient = null!;
    private BlobStorageService sut = null!;

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

        sut = fixture.Create<BlobStorageService>();
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "test content";
        var runName = "test";
        var containerName = fixture.Create<string>();
        var expectedUri = new Uri("https://example.com/test.txt");

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), true, default))
            .ReturnsAsync(new Mock<Response<BlobContentInfo>>().Object);
        mockBlobClient.Setup(x => x.Uri)
            .Returns(expectedUri);

        // Act
        var result = await sut.UploadFileContentAsync(
            (FileName: fileName,
                Content: content,
                RunName: runName,
                ContainerName: containerName,
                Overwrite: true));

        // Assert
        Assert.AreEqual(result, expectedUri.ToString());
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ShouldReturnFalse_WhenUploadFails()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "test content";
        var runName = "test";
        var containerName = fixture.Create<string>();

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestException());

        // Act & Assert
        await Should.ThrowAsync<TestException>(async () => await sut.UploadFileContentAsync(
            (FileName: fileName,
                Content: content,
                RunName: runName,
                ContainerName: containerName,
                Overwrite: true)));
    }
}
