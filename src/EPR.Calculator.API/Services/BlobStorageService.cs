using Azure;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Options;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Services;

public interface IBlobStorageService
{
    Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default);
    Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default);
    Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default);
    Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient billingCsvContainer;
    private readonly BlobContainerClient fssContainer;
    private readonly BlobContainerClient resultCsvContainer;
    private readonly BlobContainerClient billingJsonContainer;
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
        fssContainer         = blobServiceClient.GetBlobContainerClient(o.FssContainer);
        billingJsonContainer = blobServiceClient.GetBlobContainerClient(o.BillingFileJsonContainer);
    }

    public Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(resultCsvContainer, filename, cancellationToken);

    public Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(billingCsvContainer, filename, cancellationToken);    
    
    public async Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default) =>
        await OpenBlobStream(fssContainer, filename, cancellationToken);

    public Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default) =>
        MoveBlob(billingJsonContainer, fssContainer, filename, cancellationToken);

    private static async Task<Stream?> OpenBlobStream(BlobContainerClient container, string blobName, CancellationToken cancellationToken)
    {
        var blob = container.GetBlobClient(blobName);

        try
        {
            return await blob.OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
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
