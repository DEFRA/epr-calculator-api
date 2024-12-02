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
        private readonly BlobContainerClient containerClient;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection(BlobStorageSection).Get<BlobStorageSettings>() ??
                           throw new ArgumentNullException(BlobSettingsMissingError);
            containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ??
                                                                        throw new ArgumentNullException(
                                                                            ContainerNameMissingError));
            containerClient.CreateIfNotExists();
        }

        public async Task UploadResultFileContentAsync(string fileName, StringBuilder csvContent)
        {
            try
            {
                await File.WriteAllTextAsync(fileName, csvContent.ToString());
                var blobClient = containerClient.GetBlobClient(fileName);
                await using var fileStream = File.OpenRead(fileName);
                await blobClient.UploadAsync(fileStream, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while saving blob content: {ex.Message}");
            }
        }

        public async Task<string> DownloadFile(string fileName)
        {
            var blobClient = containerClient.GetBlobClient("blobName.csv");
            var response = await blobClient.DownloadAsync();
            using var streamReader = new StreamReader(response.Value.Content);
            var content = await streamReader.ReadToEndAsync();
            return content;
        }
    }
}