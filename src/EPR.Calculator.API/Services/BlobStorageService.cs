using System.Configuration;
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;

namespace EPR.Calculator.API.Services
{
    public class BlobStorageService : IStorageService
    {
        public const string BlobStorageSection = "BlobStorage";
        public const string BlobSettingsMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string AccountNameMissingError = "Account name is missing in configuration.";
        public const string OctetStream = "application/octet-stream";
        private readonly BlobContainerClient containerClient;
        private readonly StorageSharedKeyCredential sharedKeyCredential;
        private readonly ILogger<BlobStorageService> logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
                throw new ConfigurationErrorsException(BlobSettingsMissingError);

            settings.ExtractAccountDetails();

            this.sharedKeyCredential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey) ??
                throw new ConfigurationErrorsException(AccountNameMissingError);

            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                throw new ConfigurationErrorsException(ContainerNameMissingError));

            this.logger = logger;
        }

        public async Task<IResult> DownloadFile(string fileName, string blobUri)
        {
            return await this.Download(fileName, blobUri, null);
        }

        public async Task<IResult> DownloadFile(string fileName, string blobUri, string downloadFileName)
        {
            return await this.Download(fileName, blobUri, downloadFileName);
        }

        private async Task<IResult> Download(string fileName, string blobUri, string? downloadFileName = default)
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

            if (!await blobClient.ExistsAsync())
            {
                return Results.NotFound(fileName);
            }

            try
            {
                var downloadResult = await blobClient.DownloadContentAsync();
                var content = downloadResult.Value.Content.ToString();
                var destinationFileName = downloadFileName ?? fileName;
                return Results.File(Encoding.Unicode.GetBytes(content), OctetStream, destinationFileName);
            }
            catch (Exception ex)
            {
                return Results.Problem($"An error occurred while downloading the file: {ex.Message}");
            }
        }
    }
}