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
    Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient billingCsvContainer;
    private readonly BlobContainerClient billingJsonContainer;
    private readonly BlobContainerClient fssContainer;
    private readonly BlobContainerClient resultCsvContainer;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageOptions> options)
    {
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
    
    public Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default) =>
        OpenBlobStream(billingJsonContainer, filename, cancellationToken) ?? OpenBlobStream(fssContainer, filename, cancellationToken);

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
}
