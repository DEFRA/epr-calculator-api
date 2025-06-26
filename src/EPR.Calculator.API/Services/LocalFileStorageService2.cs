using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Services
{
    [ExcludeFromCodeCoverage]
    public class LocalFileStorageService2 : IBlobStorageService2
    {
        public Task<bool> MoveBlobAsync(string sourceContainer, string targetContainer, string blobName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CopyBlobAsync(string sourceContainer, string targetContainer, string blobName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBlobAsync(string container, string blobName)
        {
            throw new NotImplementedException();
        }
    }
}
