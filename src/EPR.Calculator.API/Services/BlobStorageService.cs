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
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient billingCsvContainer;
    private readonly BlobContainerClient fssContainer;
    private readonly BlobContainerClient resultCsvContainer;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageOptions> options)
    {
        var o = options.Value;
        resultCsvContainer   = blobServiceClient.GetBlobContainerClient(o.ResultFileCsvContainer);
        billingCsvContainer  = blobServiceClient.GetBlobContainerClient(o.BillingFileCsvContainer);
        fssContainer         = blobServiceClient.GetBlobContainerClient(o.FssContainer);
    }

    public Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(resultCsvContainer, filename, cancellationToken);

    public Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(billingCsvContainer, filename, cancellationToken);    
    
    public async Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default) =>
        await OpenBlobStream(fssContainer, filename, cancellationToken);

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
}
