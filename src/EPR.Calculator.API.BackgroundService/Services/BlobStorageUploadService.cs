using Azure.Storage.Blobs;
using EPR.Calculator.API.BackgroundService.Logging;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IStorageUploadService
{
    Task<string> UploadFileContentAsync(Request request, CancellationToken cancellationToken);

    public record Request
    {
        public required string FileName { get; init; }
        public required byte[] Content { get; init; }
        public required string ContainerName { get; init; }
        public bool Overwrite { get; init; }
    }
}

/// <summary>
///     Service for handling blob storage operations.
/// </summary>
public class BlobStorageUploadService(
    BlobServiceClient blobService,
    ILogger<BlobStorageUploadService> logger)
    : IStorageUploadService
{
    /// <inheritdoc />
    public Task<string> UploadFileContentAsync(
        IStorageUploadService.Request request, CancellationToken cancellationToken) =>
        logger.LogDuration(async () =>
        {
            var blobContainerClient = blobService.GetBlobContainerClient(request.ContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = blobContainerClient.GetBlobClient(request.FileName);
            var binaryData = new BinaryData(request.Content);
            await blobClient.UploadAsync(binaryData, request.Overwrite, cancellationToken);

            return blobClient.Uri.ToString();
        });
}
