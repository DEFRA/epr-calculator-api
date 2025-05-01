using System.Configuration;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.UnitTests.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageServiceTests
    {
        private readonly Mock<BlobServiceClient> mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> mockBlobContainerClient;
        private readonly Mock<BlobClient> mockBlobClient;
        private readonly Mock<ILogger<BlobStorageService>> mockLogger;
        private readonly BlobStorageService blobStorageService;

        public BlobStorageServiceTests()
        {
            this.mockBlobServiceClient = new Mock<BlobServiceClient>();
            this.mockBlobContainerClient = new Mock<BlobContainerClient>();
            this.mockBlobClient = new Mock<BlobClient>();
            var configs = ConfigurationItems.GetConfigurationValues();

            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(this.mockBlobContainerClient.Object);

            this.mockBlobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns(this.mockBlobClient.Object);

            this.mockLogger = new Mock<ILogger<BlobStorageService>>();

            this.blobStorageService = new BlobStorageService(
                this.mockBlobServiceClient.Object,
                configs,
                this.mockLogger.Object);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenBlobStorageSettingsMissing()
        {
            // Arrange
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var blobStorageSettings = new BlobStorageSettings { ContainerName = "test-container" };
            configurationSectionMock.Setup(x => x.Value).Returns(blobStorageSettings.ContainerName);
            configurationMock.Setup(x => x.GetSection("BlobStorage")).Returns(configurationSectionMock.Object);

            // Act & Assert is handled by ExpectedException
            Assert.ThrowsException<ConfigurationErrorsException>(
                () => new BlobStorageService(
                    this.mockBlobServiceClient.Object,
                    configurationMock.Object,
                    this.mockLogger.Object));
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

            this.mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null!));
            this.mockBlobClient.Setup(x => x.DownloadContentAsync()).ReturnsAsync(Response.FromValue(downloadResult, null!));
            this.mockBlobClient.Setup(x => x.Uri).Returns(new Uri(blobUri));
            blobUri = string.Empty;

            // Act
            var result = await this.blobStorageService.DownloadFile(fileName, blobUri);

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
            var blobUri = string.Empty;
            this.mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(false, null!));

            // Act
            var result = await this.blobStorageService.DownloadFile(fileName, blobUri);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFound<string>));
            var notFoundResult = result as NotFound<string>;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(fileName, notFoundResult.Value);
        }

        [TestMethod]
        public async Task DownloadFile_WithRequestedDownLoadFileName_ShouldReturnFileResult_WhenFileExists()
        {
            // Arrange
            var fileName = "test.txt";
            var downLoadFileName = "downloadfile.txt";
            var blobUri = "https://example.com/test.txt";
            var content = "test content";
            var binaryData = BinaryData.FromString(content);
            var downloadDetails = BlobsModelFactory.BlobDownloadDetails(
                contentLength: content.Length,
                contentType: "application/octet-stream");

            var downloadResult = BlobsModelFactory.BlobDownloadResult(
                content: binaryData,
                details: downloadDetails);

            this.mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(true, null!));
            this.mockBlobClient.Setup(x => x.DownloadContentAsync()).ReturnsAsync(Response.FromValue(downloadResult, null!));
            this.mockBlobClient.Setup(x => x.Uri).Returns(new Uri(blobUri));
            blobUri = string.Empty;

            // Act
            var result = await this.blobStorageService.DownloadFile(fileName, blobUri, downLoadFileName);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentHttpResult));
            var fileContentResult = (FileContentHttpResult)result;
            Assert.AreEqual("application/octet-stream", fileContentResult.ContentType);
            Assert.AreEqual(downLoadFileName, fileContentResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFile_WithRequestedDownLoadFileName_ShouldReturnNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var fileName = "test.txt";
            var downLoadFileName = "downloadfile.txt";
            var blobUri = string.Empty;
            this.mockBlobClient.Setup(x => x.ExistsAsync(default)).ReturnsAsync(Response.FromValue(false, null!));

            // Act
            var result = await this.blobStorageService.DownloadFile(fileName, blobUri,downLoadFileName);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFound<string>));
            var notFoundResult = result as NotFound<string>;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(fileName, notFoundResult.Value);
        }
    }
}