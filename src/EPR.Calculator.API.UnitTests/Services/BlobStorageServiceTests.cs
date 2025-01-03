using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Configuration;

namespace EPR.Calculator.API.UnitTests.Services
{
  [TestClass]
  public class BlobStorageServiceTests
  {
    private Mock<BlobServiceClient> _mockBlobServiceClient;
    private Mock<BlobContainerClient> _mockBlobContainerClient;
    private Mock<BlobClient> _mockBlobClient;
    private BlobStorageService _blobStorageService;

    [TestInitialize]
    public void Setup()
    {
      _mockBlobServiceClient = new Mock<BlobServiceClient>();
      _mockBlobContainerClient = new Mock<BlobContainerClient>();
      _mockBlobClient = new Mock<BlobClient>();
      var configs = ConfigurationItems.GetConfigurationValues();

      _mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
          .Returns(_mockBlobContainerClient.Object);

      _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
          .Returns(_mockBlobClient.Object);

      _blobStorageService = new BlobStorageService(_mockBlobServiceClient.Object, configs);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      _mockBlobServiceClient = null;
      _mockBlobContainerClient = null;
      _mockBlobClient = null;
      _blobStorageService = null;
    }

    [TestMethod]
    public void Constructor_ShouldThrowException_WhenBlobStorageSettingsMissing()
    {
      // Arrange
      var _configurationMock = new Mock<IConfiguration>();
      var configurationSectionMock = new Mock<IConfigurationSection>();
      var blobStorageSettings = new BlobStorageSettings { ContainerName = "test-container" };
      configurationSectionMock.Setup(x => x.Value).Returns(blobStorageSettings.ContainerName);
      _configurationMock.Setup(x => x.GetSection("BlobStorage")).Returns(configurationSectionMock.Object);

      // Act & Assert is handled by ExpectedException
      Assert.ThrowsException<ConfigurationErrorsException>(() => new BlobStorageService(_mockBlobServiceClient.Object, _configurationMock.Object));
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
    {
      // Arrange
      var fileName = "test.txt";
      var content = "test content";

      var responseMock = new Mock<Response<BlobContentInfo>>();
      _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
                    .ReturnsAsync(responseMock.Object);

      // Act
      var result = await _blobStorageService.UploadResultFileContentAsync(fileName, content);

      // Assert
      Assert.IsTrue(result);
      _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>()), Times.Once);
    }

    [TestMethod]
    public async Task UploadResultFileContentAsync_ShouldReturnFalse_WhenUploadFails()
    {
      // Arrange
      var fileName = "test.txt";
      var content = "test content";

      _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
          .ThrowsAsync(new Exception("Upload failed"));

      // Act
      var result = await _blobStorageService.UploadResultFileContentAsync(fileName, content);

      // Assert
      Assert.IsFalse(result);
      _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>()), Times.Once);
    }

    [TestMethod]
    public async Task DownloadFile_ShouldReturnFileResult_WhenFileExists()
    {
      // Arrange
      var fileName = "test.txt";
      var content = "test content";
      var binaryData = BinaryData.FromString(content);
      var downloadDetails = BlobsModelFactory.BlobDownloadDetails(
          contentLength: content.Length,
          contentType: "application/octet-stream");

      var downloadResult = BlobsModelFactory.BlobDownloadResult(
          content: binaryData,
          details: downloadDetails);

      _mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null));
      _mockBlobClient.Setup(x => x.DownloadContentAsync()).ReturnsAsync(Response.FromValue(downloadResult, null));

      // Act
      var result = await _blobStorageService.DownloadFile(fileName);

      // Assert
      Assert.IsInstanceOfType(result, typeof(FileContentHttpResult));
      var fileContentResult = (FileContentHttpResult)result;
      Assert.AreEqual("application/octet-stream", fileContentResult.ContentType);
      Assert.AreEqual(fileName, fileContentResult.FileDownloadName);
    }

    [TestMethod]
    public async Task DownloadFile_ShouldReturnNotFound_WhenFileDoesNotExist()
    {
      // Arrange
      var fileName = "test.txt";
      _mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(false, null));

      // Act
      var result = await _blobStorageService.DownloadFile(fileName);

      // Assert
      Assert.IsInstanceOfType(result, typeof(NotFound<string>));
      var notFoundObjectResult = (NotFound<string>)result;
      Assert.AreEqual(fileName, notFoundObjectResult.Value);
    }
  }
}
