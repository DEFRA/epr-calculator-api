using System.Text;
using Azure.Storage.Blobs;
using EPR.Calculator.API.BackgroundService.Logging;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IStorageUploadService
{
    Task<string> UploadFileContentAsync(Request request, CancellationToken cancellationToken);

    public record Request
    {
        public required string FileName { get; init; }
        public required string Content { get; init; }
        public required string ContainerName { get; init; }
        public bool Overwrite { get; init; }
        public bool UseUtf8Bom { get; init; } = true;
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
            var binaryData = new BinaryData(GetContentBytes(request.Content, request.UseUtf8Bom));
            await blobClient.UploadAsync(binaryData, request.Overwrite, cancellationToken);

            return blobClient.Uri.ToString();
        });

    /// <summary>
    ///     Encodes <paramref name="content"/> as UTF-8, optionally prefixed with a byte order mark (BOM)
    ///     so that tools such as Excel correctly detect the encoding when opening the file (e.g. CSVs).
    /// </summary>
    private static byte[] GetContentBytes(string content, bool useUtf8Bom)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);

        if (!useUtf8Bom)
            return contentBytes;

        var bom = Encoding.UTF8.GetPreamble();
        var bytesWithBom = new byte[bom.Length + contentBytes.Length];
        Buffer.BlockCopy(bom, 0, bytesWithBom, 0, bom.Length);
        Buffer.BlockCopy(contentBytes, 0, bytesWithBom, bom.Length, contentBytes.Length);
        return bytesWithBom;
    }
}
