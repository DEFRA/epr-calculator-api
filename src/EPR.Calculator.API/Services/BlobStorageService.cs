using Azure.Storage;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using System.Configuration;
using System.Text;
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

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
                throw new ConfigurationErrorsException(BlobSettingsMissingError);
            
            settings.ExtractAccountDetails();
           
            this.sharedKeyCredential = new StorageSharedKeyCredential(settings.AccountName, settings.AccountKey) ??
                throw new ConfigurationErrorsException(AccountNameMissingError);

            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                throw new ConfigurationErrorsException(ContainerNameMissingError));
        }

        public async Task<string> UploadResultFileContentAsync(string fileName, string content)
        {
            try
            {
                var blobClient = this.containerClient.GetBlobClient(fileName);
                var binaryData = BinaryData.FromString(content);
                await blobClient.UploadAsync(binaryData);
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<IResult> DownloadFile(string fileName, string blobUri)
        {
            BlobClient blobClient;
            try
            {
                blobClient = new BlobClient(new Uri(blobUri), sharedKeyCredential);
            }
            catch
            {
                blobClient = this.containerClient.GetBlobClient(fileName);
            }
            
            if (!await blobClient.ExistsAsync())
            {
                return Results.NotFound(fileName);
            }

            var downloadResult = await blobClient.DownloadContentAsync();
            var content = downloadResult.Value.Content.ToString();
            return Results.File(Encoding.Unicode.GetBytes(content), OctetStream, fileName);
        }
    }
}