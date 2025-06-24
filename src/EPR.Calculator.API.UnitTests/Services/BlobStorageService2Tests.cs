using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPR.Calculator.API.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Services
{
    [TestClass]
    public class BlobStorageService2Tests
    {
        private readonly BlobStorageService2 blobStorageService2;
        private readonly Mock<BlobServiceClient> mockBlobServiceClient;
        private readonly Mock<BlobContainerClient> mockSourceContainerClient;
        private readonly Mock<BlobContainerClient> mockDestContainerClient;
        private readonly Mock<BlobClient> mockSourceBlobClient;
        private readonly Mock<BlobClient> mockDestBlobClient;

        public BlobStorageService2Tests()
        {
            this.mockBlobServiceClient = new Mock<BlobServiceClient>();
            this.mockSourceContainerClient = new Mock<BlobContainerClient>();
            this.mockDestContainerClient = new Mock<BlobContainerClient>();
            this.mockSourceBlobClient = new Mock<BlobClient>();
            this.mockDestBlobClient = new Mock<BlobClient>();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "BlobStorage:ContainerName", "ContainerName" },
                { "BlobStorage:ConnectionString", "UseDevelopmentStorage=true" },
                { "BlobStorage:BillingFileCSVContainerName", "BillingFileCSVContainerName" },
                { "BlobStorage:BillingFileJsonContainerName", "BillingFileJsonContainerName" },
                { "BlobStorage:BillingFileJsonForFssContainerName", "BillingFileJsonForFssContainerName" },
            };

            var configs = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            this.blobStorageService2 = new BlobStorageService2(configs, new TelemetryClient());

            // Patch the private blobServiceClient field to use the mock
            var field = typeof(BlobStorageService2).GetField("blobServiceClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field is null)
            {
                throw new InvalidOperationException("Could not find the 'blobServiceClient' field on BlobStorageService2. Check the field name and its accessibility.");
            }

            field.SetValue(this.blobStorageService2, this.mockBlobServiceClient.Object);
        }

        [TestMethod]
        public async Task CopyBlobAsync_ReturnsTrue_WhenCopySucceeds()
        {
            // Arrange
            var sourceContainer = "source";
            var destContainer = "dest";
            var blobName = "file.txt";

            // Set up the container and blob mocks
            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(sourceContainer)).Returns(this.mockSourceContainerClient.Object);
            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(destContainer)).Returns(this.mockDestContainerClient.Object);

            this.mockSourceContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockSourceBlobClient.Object);
            this.mockDestContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockDestBlobClient.Object);
            this.mockDestContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Azure.Response<BlobContainerInfo>)null);

            // Setup the copy operation mock
            var mockCopyOperation = new Mock<CopyFromUriOperation>();
            mockCopyOperation.Setup(x => x.WaitForCompletionAsync(It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Response<long>>(Response.FromValue(0L, (Response)null)));
            mockCopyOperation.Setup(x => x.HasCompleted).Returns(true);

            this.mockDestBlobClient
                .Setup(x => x.StartCopyFromUriAsync(It.IsAny<Uri>(), null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCopyOperation.Object);

            // Act
            var result = await this.blobStorageService2.CopyBlobAsync(sourceContainer, destContainer, blobName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CopyBlobAsync_ReturnsFalse_WhenCopyThrows()
        {
            // Arrange
            var sourceContainer = "source";
            var destContainer = "dest";
            var blobName = "file.txt";

            this.mockSourceContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockSourceBlobClient.Object);
            this.mockDestContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockDestBlobClient.Object);
            this.mockDestContainerClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Azure.Response<BlobContainerInfo>)null);

            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(sourceContainer)).Returns(this.mockSourceContainerClient.Object);
            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(destContainer)).Returns(this.mockDestContainerClient.Object);

            this.mockDestBlobClient.Setup(x => x.StartCopyFromUriAsync(It.IsAny<Uri>(), null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Copy failed"));

            // Act
            var result = await this.blobStorageService2.CopyBlobAsync(sourceContainer, destContainer, blobName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task DeleteBlobAsync_ReturnsTrue_WhenDeleteSucceeds()
        {
            // Arrange
            var container = "source";
            var blobName = "file.txt";

            // Setup the container and blob mocks
            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(container)).Returns(this.mockSourceContainerClient.Object);
            this.mockSourceContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockSourceBlobClient.Object);
            this.mockSourceBlobClient.Setup(x => x.DeleteIfExistsAsync(
                    It.IsAny<DeleteSnapshotsOption>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, (Response)null));

            // Act
            var result = await this.blobStorageService2.DeleteBlobAsync(container, blobName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteBlobAsync_ReturnsFalse_WhenDeleteThrows()
        {
            // Arrange
            var container = "source";
            var blobName = "file.txt";

            this.mockSourceBlobClient.Setup(x => x.DeleteIfExistsAsync(It.IsAny<DeleteSnapshotsOption>(), null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Delete failed"));

            this.mockSourceContainerClient.Setup(x => x.GetBlobClient(blobName)).Returns(this.mockSourceBlobClient.Object);
            this.mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(container)).Returns(this.mockSourceContainerClient.Object);

            // Act
            var result = await this.blobStorageService2.DeleteBlobAsync(container, blobName);

            // Assert
            Assert.IsFalse(result);
        }
    }
}