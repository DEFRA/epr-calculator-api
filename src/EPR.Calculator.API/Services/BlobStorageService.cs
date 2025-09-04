using System.Configuration;
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.Services
{
    public class BlobStorageService : IStorageService
    {
        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ILogger<BlobStorageService> logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            var settings = configuration.GetSection(CommonResources.BlobStorageSection).Get<BlobStorageSettings>() ??
                throw new ConfigurationErrorsException(CommonResources.BlobSettingsMissingError);

            settings.ExtractAccountDetails();

            this.sharedKeyCredential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey) ??
                throw new ConfigurationErrorsException(CommonResources.AccountNameMissingError);

            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ResultFileCSVContainerName ??
                throw new ConfigurationErrorsException(CommonResources.ContainerNameMissingError));

            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IResult> DownloadFile(string fileName, string blobUri)
        {
            BlobClient blobClient = this.GetBlobClient(fileName, blobUri);

            if (!await blobClient.ExistsAsync())
            {
                return Results.NotFound(fileName);
            }

            try
            {
                var downloadResult = await blobClient.DownloadContentAsync();
                var content = downloadResult.Value.Content.ToString();
                return Results.File(Encoding.Unicode.GetBytes(content), CommonResources.OctetStream, fileName);
            }
            catch (Exception ex)
            {
                return Results.Problem(string.Format(CommonResources.DownloadFileError, ex.Message));
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsBlobExistsAsync(string fileName, string blobUri, CancellationToken cancellationToken)
        {
            BlobClient blobClient = this.GetBlobClient(fileName, blobUri);

            return await blobClient.ExistsAsync(cancellationToken);
        }

        private BlobClient GetBlobClient(string fileName, string blobUri)
        {
            BlobClient? blobClient = null;

            if (!string.IsNullOrEmpty(blobUri))
            {
                try
                {
                    blobClient = new BlobClient(new Uri(blobUri), this.sharedKeyCredential);
                }
                catch (UriFormatException exception)
                {
                    this.logger.LogError(exception, "Blob Uri is not in correct format.");
                    blobClient ??= this.containerClient.GetBlobClient(fileName);
                }
            }
            else
            {
                blobClient ??= this.containerClient.GetBlobClient(fileName);
            }

            return blobClient;
        }
    }
}