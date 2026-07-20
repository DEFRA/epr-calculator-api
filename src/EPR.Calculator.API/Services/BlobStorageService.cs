using System.Configuration;
using System.Text;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.Services;

public class BlobStorageService : IStorageService
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly BlobContainerClient containerClient;
    private readonly ILogger<BlobStorageService> logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        var settings = configuration
            .GetSection(CommonResources.BlobStorageSection)
            .Get<BlobStorageSettings>() ?? throw new ConfigurationErrorsException(CommonResources.BlobSettingsMissingError);
        this.blobServiceClient = blobServiceClient;

        containerClient = blobServiceClient
            .GetBlobContainerClient(settings.ResultFileCSVContainerName ?? throw new ConfigurationErrorsException(CommonResources.ContainerNameMissingError));

        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult> DownloadFile(string fileName, string blobUri)
    {
        var blobClient = GetBlobClient(fileName, blobUri);

        if (!await blobClient.ExistsAsync())
            return Results.NotFound(fileName);

        var downloadResult = await blobClient.DownloadContentAsync();
        var content = downloadResult.Value.Content.ToString();
        return Results.File(Encoding.Unicode.GetBytes(content), CommonResources.OctetStream, fileName);
    }

    private BlobClient GetBlobClient(string fileName, string blobUri)
    {
        if (!string.IsNullOrEmpty(blobUri))
        {
            try
            {
                var blobUriBuilder = new BlobUriBuilder(new Uri(blobUri));

                if (!string.IsNullOrWhiteSpace(blobUriBuilder.BlobContainerName) && !string.IsNullOrWhiteSpace(blobUriBuilder.BlobName))
                    return blobServiceClient.GetBlobContainerClient(blobUriBuilder.BlobContainerName).GetBlobClient(blobUriBuilder.BlobName);

                logger.LogWarning("Blob Uri is missing container or blob name; falling back to configured container");
            }
            catch (UriFormatException exception)
            {
                logger.LogError(exception, "Blob Uri is not in correct format");
            }
        }

        return containerClient.GetBlobClient(fileName);
    }
}
