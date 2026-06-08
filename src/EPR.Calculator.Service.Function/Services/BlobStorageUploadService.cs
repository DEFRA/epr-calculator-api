using Azure.Storage.Blobs;
using EPR.Calculator.Service.Function.Logging;

namespace EPR.Calculator.Service.Function.Services;

public interface IStorageUploadService
{
    Task<string> UploadFileContentAsync((string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args, CancellationToken cancellationToken);
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
        (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args, CancellationToken cancellationToken) =>
        logger.LogDuration(async () =>
        {
            var blobContainerClient = blobService.GetBlobContainerClient(args.ContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = blobContainerClient.GetBlobClient(args.FileName);
            var binaryData = BinaryData.FromString(args.Content);
            await blobClient.UploadAsync(binaryData, args.Overwrite, cancellationToken);

            return blobClient.Uri.ToString();
        });
}
