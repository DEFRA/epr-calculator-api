using System.Text;
using Azure.Storage.Blobs;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IStorageUploadService
{
    // For callers whose content is already a whole string (e.g. CSV export, built via StringBuilder)
    Task<string> UploadFileContentAsync(Request request, CancellationToken cancellationToken);

    // Streams straight to blob storage.
    Task<string> UploadFileStreamAsync(StreamRequest request, Func<Stream, CancellationToken, Task> writeContent, CancellationToken cancellationToken);

    public record Request
    {
        public required string FileName { get; init; }
        public required byte[] Content { get; init; }
        public required string ContainerName { get; init; }
        public bool Overwrite { get; init; }
    }

    public record StreamRequest
    {
        public required string FileName { get; init; }
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
        var binaryData = new BinaryData(request.Content);
        await blobClient.UploadAsync(binaryData, request.Overwrite, cancellationToken);

        return blobClient.Uri.ToString();
    }

    [ActivityTrace]
    public async Task<string> UploadFileStreamAsync(IStorageUploadService.StreamRequest request, Func<Stream, CancellationToken, Task> writeContent, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobService.GetBlobContainerClient(request.ContainerName);

        if (!await blobContainerClient.ExistsAsync(cancellationToken))
            await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = blobContainerClient.GetBlobClient(request.FileName);

        await using (var blobStream = await blobClient.OpenWriteAsync(request.Overwrite, options: null, cancellationToken))
        {
            if (request.UseUtf8Bom)
                await blobStream.WriteAsync(Encoding.UTF8.GetPreamble(), cancellationToken);

            await writeContent(blobStream, cancellationToken);
            await blobStream.FlushAsync(cancellationToken);
        }

        return blobClient.Uri.ToString();
    }
}
