namespace EPR.Calculator.API.Services;

public interface IBlobStorageService2
{
    Task<bool> MoveBlobAsync(string sourceContainer, string targetContainer, string blobName);

    Task<bool> CopyBlobAsync(string sourceContainer, string targetContainer, string blobName);

    Task<bool> DeleteBlobAsync(string container, string blobName);
}