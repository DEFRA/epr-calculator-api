using Azure.Storage.Blobs;
using EPR.Calculator.API.Utils;
using Microsoft.ApplicationInsights;

namespace EPR.Calculator.API.Services
{
    public class BlobStorageService2 : IBlobStorageService2
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly TelemetryClient telemetryClient;

        public BlobStorageService2(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            var settings = new BlobStorageSettings();
            configuration.GetSection("BlobStorage").Bind(settings);
            this.blobServiceClient = new BlobServiceClient(settings.ConnectionString);
            this.telemetryClient = telemetryClient;
        }

        public async Task<bool> MoveBlobAsync(string sourceContainer, string targetContainer, string blobName)
        {
            var copySucceeded = await this.CopyBlobAsync(sourceContainer, targetContainer, blobName);
            if (!copySucceeded)
            {
                return false;
            }

            var deleteSucceeded = await this.DeleteBlobAsync(sourceContainer, blobName);

            // Log if delete fails, but still return true if copy succeeded
            if (!deleteSucceeded)
            {
                this.telemetryClient.TrackTrace(string.Format(CommonResources.BlobDeleteUnsuccessful, blobName));
            }

            return true;
        }

        public async Task<bool> CopyBlobAsync(string sourceContainer, string targetContainer, string blobName)
        {
            var sourceContainerClient = this.blobServiceClient.GetBlobContainerClient(sourceContainer);
            var destContainerClient = this.blobServiceClient.GetBlobContainerClient(targetContainer);

            await destContainerClient.CreateIfNotExistsAsync();

            var sourceBlob = sourceContainerClient.GetBlobClient(blobName);
            var destBlob = destContainerClient.GetBlobClient(blobName);

            try
            {
                var copyOperation = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                await copyOperation.WaitForCompletionAsync();

                return copyOperation.HasCompleted;
            }
            catch (Exception e)
            {
                this.telemetryClient.TrackTrace($"Exception copying blob for: {blobName} with exception :{e.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteBlobAsync(string container, string blobName)
        {
            var containerClient = this.blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                this.telemetryClient.TrackTrace($"Exception deleting blob for: {blobName} with exception :{e.Message}");
                return false;
            }
        }
    }
}