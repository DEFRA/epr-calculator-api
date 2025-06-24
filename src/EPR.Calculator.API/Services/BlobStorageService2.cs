using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using Microsoft.ApplicationInsights;

namespace EPR.Calculator.API.Services
{
    public class BlobStorageService2 : IBlobStorageService2
    {
        private readonly BlobStorageSettings settings;
        private readonly BlobServiceClient blobServiceClient;
        private readonly TelemetryClient telemetryClient;

        public BlobStorageService2(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            this.settings = new BlobStorageSettings();
            configuration.GetSection("BlobStorage").Bind(this.settings);
            this.blobServiceClient = new BlobServiceClient(this.settings.ConnectionString);
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
                this.telemetryClient.TrackTrace($"Blob delete for: {blobName} unsuccessful");
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
                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
                return await this.PollForCopyCompletionAsync(destBlob, blobName);
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

        private async Task<bool> PollForCopyCompletionAsync(BlobClient destBlob, string blobName)
        {
            while (true)
            {
                var properties = await destBlob.GetPropertiesAsync();
                var status = properties.Value.CopyStatus;

                if (status == Azure.Storage.Blobs.Models.CopyStatus.Success)
                {
                    return true;
                }

                if (status == Azure.Storage.Blobs.Models.CopyStatus.Failed ||
                    status == Azure.Storage.Blobs.Models.CopyStatus.Aborted)
                {
                    this.telemetryClient.TrackTrace($"Copy failed or aborted for: {blobName}");
                    return false;
                }

                await Task.Delay(500);
            }
        }
    }
}