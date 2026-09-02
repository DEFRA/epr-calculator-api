using System.Text;
using Azure.Storage.Blobs;

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
    BlobServiceClient blobService
) : IStorageUploadService
{
    [ActivityTrace]
    public async Task<string> UploadFileContentAsync(IStorageUploadService.Request request, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobService.GetBlobContainerClient(request.ContainerName);

        // Checking first avoids CreateIfNotExistsAsync's 409 "already exists" response, which the underlying
        // Azure SDK pipeline logs as a warning on every call regardless of it being expected/handled.
        if (!await blobContainerClient.ExistsAsync(cancellationToken))
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = blobContainerClient.GetBlobClient(request.FileName);
        var binaryData = new BinaryData(GetContentBytes(request.Content, request.UseUtf8Bom));
        await blobClient.UploadAsync(binaryData, request.Overwrite, cancellationToken);

        return blobClient.Uri.ToString();
    }

    /// <summary>
    ///     Encodes <paramref name="content" /> as UTF-8, optionally prefixed with a byte order mark (BOM)
    ///     so that tools such as Excel correctly detect the encoding when opening the file (e.g. CSVs).
    /// </summary>
    private static byte[] GetContentBytes(string content, bool useUtf8Bom)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);

        if (!useUtf8Bom)
            return contentBytes;

        var bom = Encoding.UTF8.GetPreamble();
        var bytesWithBom = new byte[bom.Length + contentBytes.Length];
        Buffer.BlockCopy(bom, srcOffset: 0, bytesWithBom, dstOffset: 0, bom.Length);
        Buffer.BlockCopy(contentBytes, srcOffset: 0, bytesWithBom, bom.Length, contentBytes.Length);
        return bytesWithBom;
    }
}
