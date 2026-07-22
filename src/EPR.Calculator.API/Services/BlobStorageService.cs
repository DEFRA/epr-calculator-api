using System.Text;
using Azure;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Options;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Services;

public interface IBlobStorageService
{
    Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default);
    Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default);
    Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient billingCsvContainer;
    private readonly BlobContainerClient billingJsonContainer;
    private readonly BlobContainerClient fssContainer;
    private readonly BlobContainerClient resultCsvContainer;
    private readonly ILogger<BlobStorageService> logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        this.logger = logger;
        var o = options.Value;
        resultCsvContainer   = blobServiceClient.GetBlobContainerClient(o.ResultFileCsvContainer);
        billingCsvContainer  = blobServiceClient.GetBlobContainerClient(o.BillingFileCsvContainer);
        billingJsonContainer = blobServiceClient.GetBlobContainerClient(o.BillingFileJsonContainer);
        fssContainer         = blobServiceClient.GetBlobContainerClient(o.FssContainer);
    }

    public Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(resultCsvContainer, filename, cancellationToken);

    public Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(billingCsvContainer, filename, cancellationToken);

    public Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default) =>
        MoveBlob(billingJsonContainer, fssContainer, filename, cancellationToken);

    private static async Task<Stream?> OpenBlobStream(BlobContainerClient container, string blobName, CancellationToken cancellationToken)
    {
        var blob = container.GetBlobClient(blobName);

        try
        {
            var blobStream = await blob.OpenReadAsync(cancellationToken: cancellationToken);

            // Preserves existing behaviour - not entirely sure why this is necessary.
            // Files are currently written/stored as UTF-8.
            // If UTF-16 is important, files should be written as such to begin with.
            // This method would then just return blobStream rather than having to re-encode.
            return await ReEncodeAsUtf16Async(blobStream, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    private static async Task<MemoryStream> ReEncodeAsUtf16Async(Stream source, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(source, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync(cancellationToken);

        var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, new UnicodeEncoding(bigEndian: false, byteOrderMark: true), leaveOpen: true);
        await writer.WriteAsync(content.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<bool> MoveBlob(BlobContainerClient source, BlobContainerClient destination, string blobName, CancellationToken cancellationToken)
    {
        if (!await CopyBlob(source, destination, blobName, cancellationToken))
            return false;

        // Cleanup failure is a warning, not a move failure — the blob is already in the destination.
        await DeleteBlob(source, blobName, cancellationToken);
        return true;
    }

    private async Task<bool> CopyBlob(BlobContainerClient source, BlobContainerClient destination, string blobName, CancellationToken cancellationToken)
    {
        var sourceBlob = source.GetBlobClient(blobName);

        if (!await sourceBlob.ExistsAsync(cancellationToken))
            return false;

        try
        {
            await destination.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            var destBlob = destination.GetBlobClient(blobName);
            var copyOperation = await destBlob.StartCopyFromUriAsync(sourceBlob.Uri, cancellationToken: cancellationToken);
            await copyOperation.WaitForCompletionAsync(cancellationToken);
            return true;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Unable to copy blob {BlobName} ({ErrorCode}) from {SourceContainer} -> {DestinationContainer}", blobName, ex.ErrorCode, source.Name, destination.Name);
            return false;
        }
    }

    private async Task DeleteBlob(BlobContainerClient container, string blobName, CancellationToken cancellationToken)
    {
        var blobClient = container.GetBlobClient(blobName);

        if (!await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken))
            logger.LogWarning("Unable to delete blob {BlobName} from {SourceContainer}", blobName, container.Name);
    }
}
