using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;

namespace EPR.Calculator.API.Services
{
    public class BlobStorageService2 : IBlobStorageService2
    {
        private readonly BlobStorageSettings settings;

        public BlobStorageService2(IConfiguration configuration)
        {
            this.settings = new BlobStorageSettings();
            configuration.GetSection("BlobStorage").Bind(this.settings);
        }

        public async Task<bool> MoveBlobAsync(string sourceContainer, string targetContainer, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(this.settings.ConnectionString);
            var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainer);
            var destContainerClient = blobServiceClient.GetBlobContainerClient(targetContainer);

            // Ensure the destination container exists
            await destContainerClient.CreateIfNotExistsAsync();

            var sourceBlob = sourceContainerClient.GetBlobClient(blobName);
            var destBlob = destContainerClient.GetBlobClient(blobName);

            try
            {
                // Start copy
                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                // Delete source
                await sourceBlob.DeleteIfExistsAsync();

                return true;
            }
            catch (Exception e)
            {
                // TODO telemetry here?
                return false;
            }
        }
    }
}