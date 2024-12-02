using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using System.Text;
namespace EPR.Calculator.API.Services
{
    public class BlobStorageService : IStorageService
    {
        public const string BlobStorageSection = "BlobStorage";
        public const string BlobSettingsMissingError = "BlobStorage settings are missing in configuration.";
        public const string ContainerNameMissingError = "Container name is missing in configuration.";
        public const string OctetStream = "application/octet-stream";
        private readonly BlobContainerClient containerClient;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
                           throw new ArgumentNullException(BlobSettingsMissingError);
            this.containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                                                                        throw new ArgumentNullException(
                                                                            ContainerNameMissingError));
        }

        public async Task UploadResultFileContentAsync(string fileName, StringBuilder csvContent)
        {
            try
            {
                await File.WriteAllTextAsync(fileName, csvContent.ToString());
                var blobClient = this.containerClient.GetBlobClient(fileName);
                await using var fileStream = File.OpenRead(fileName);
                await blobClient.UploadAsync(fileStream, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while saving blob content: {ex.Message}");
            }
        }

        public async Task<IResult> DownloadFile(string fileName)
        {
            using var memoryStream = new MemoryStream();
            var blobClient = this.containerClient.GetBlobClient(fileName);
            await blobClient.DownloadToAsync(memoryStream);
            return Results.File(memoryStream.ToArray(), OctetStream, fileName);
        }
    }
}