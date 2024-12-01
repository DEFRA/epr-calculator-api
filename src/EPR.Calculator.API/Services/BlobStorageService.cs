using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using System.Text;
namespace EPR.Calculator.API.Services
{
    public class BlobStorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection("BlobStorage").Get<BlobStorageSettings>() ?? throw new ArgumentNullException("BlobStorage settings are missing in configuration.");
            _containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ?? throw new ArgumentNullException("Container name is missing in configuration."));
        }

        public async Task UploadResultFileContentAsync(string fileName, StringBuilder csvContent)
        {
            try
            {
                File.WriteAllText(fileName, csvContent.ToString());
                var blobClient = _containerClient.GetBlobClient(fileName);
                using var fileStream = File.OpenRead(fileName);
                await blobClient.UploadAsync(fileStream, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while saving blob content: {ex.Message}");
            }
        }
    }
}