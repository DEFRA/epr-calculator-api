using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using System.Text;
namespace EPR.Calculator.API.Services
{
    public class BlobStorageService: IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly BlobClient _blobClient;

        public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            var settings = configuration.GetSection("AzureBlobStorage").Get<BlobStorageSettings>() ?? throw new ArgumentNullException("AzureBlobStorage settings are missing in configuration.");
            _containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName ?? throw new ArgumentNullException("Container name is missing in configuration."));
            _blobClient = _containerClient.GetBlobClient(settings.CsvFileName ?? throw new ArgumentNullException("CSV file name is missing in configuration."));
        }

        public async Task<string?> GetResultFileContentAsync()
        {
            try
            {
                if (await _blobClient.ExistsAsync())
                {
                    var downloadInfo = await _blobClient.DownloadAsync();
                    using var streamReader = new StreamReader(downloadInfo.Value.Content);
                    return await streamReader.ReadToEndAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while retrieving blob content: {ex.Message}");
                return null;
            }
        }

        public async Task UploadResultFileContentAsync(string fileName, StringBuilder csvContent)
        {
            try
            {
                File.WriteAllText(fileName, csvContent.ToString());
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
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