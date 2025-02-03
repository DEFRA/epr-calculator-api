using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageServiceTests
    {
        private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly Mock<ILogger<BlobStorageService>> _mockLogger;
        private readonly BlobStorageService _blobStorageService;

        public BlobStorageServiceTests()
        {
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();
            var configs = ConfigurationItems.GetConfigurationValues();

            _mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_mockBlobContainerClient.Object);

            _mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockLogger = new Mock<ILogger<BlobStorageService>>();

            _blobStorageService = new BlobStorageService(_mockBlobServiceClient.Object, configs, _mockLogger.Object);
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
            Assert.ThrowsException<ConfigurationErrorsException>(() => new BlobStorageService(_mockBlobServiceClient.Object, _configurationMock.Object, _mockLogger.Object));
        }

        [TestMethod]
        public async Task UploadResultFileContentAsync_ReturnsTrue_WhenUploadSucceeds()
        {
            // Arrange
            var fileName = "test.txt";
            var content = "test content";
            var expectedUri = new Uri("https://example.com/test.txt");

            var responseMock = new Mock<Response<BlobContentInfo>>();
            _mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<BinaryData>()))
                          .ReturnsAsync(responseMock.Object);
            _mockBlobClient.Setup(x => x.Uri).Returns(expectedUri);

            // Act
            var result = await _blobStorageService.UploadResultFileContentAsync(fileName, content);

            // Assert
            Assert.AreEqual(result, expectedUri.ToString());
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
            Assert.AreEqual(result, string.Empty);
            _mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<BinaryData>()), Times.Once);
        }

        [TestMethod]
        public async Task DownloadFile_ShouldReturnFileResult_WhenFileExists()
        {
            // Arrange
            var fileName = "test.txt";
            var blobUri = "https://example.com/test.txt";
            var content = "test content";
            var binaryData = BinaryData.FromString(content);
            var downloadDetails = BlobsModelFactory.BlobDownloadDetails(
                contentLength: content.Length,
                contentType: "application/octet-stream");

            var downloadResult = BlobsModelFactory.BlobDownloadResult(
                content: binaryData,
                details: downloadDetails);

            _mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null!));
            _mockBlobClient.Setup(x => x.DownloadContentAsync()).ReturnsAsync(Response.FromValue(downloadResult, null!));
            _mockBlobClient.Setup(x => x.Uri).Returns(new Uri(blobUri));
            blobUri = "";

            // Act
            var result = await _blobStorageService.DownloadFile(fileName, blobUri);

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
            var blobUri = "";
            _mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(false, null!));

            // Act
            var result = await _blobStorageService.DownloadFile(fileName, blobUri);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFound<string>));
            var notFoundResult = result as NotFound<string>;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(fileName, notFoundResult.Value);
        }
    }
}