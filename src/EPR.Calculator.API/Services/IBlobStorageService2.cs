namespace EPR.Calculator.API.Services;

public interface IBlobStorageService2
{
    Task<bool> MoveBlobAsync(string sourceContainer, string targetContainer, string blobName);
}