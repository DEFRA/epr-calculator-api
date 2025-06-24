using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EPR.Calculator.API.Services
{
    [ExcludeFromCodeCoverage]
    public class LocalFileStorageService : IStorageService
    {
        public Task<string?> GetResultFileContentAsync()
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadResultFileContentAsync(string fileName, string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(path, content, Encoding.UTF8);
            return Task.FromResult(path);
        }

        public Task<IResult> DownloadFile(string fileName, string blobUri)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsBlobExistsAsync(string fileName, string blobUri, CancellationToken cancellationToken)
        {
            // TODO DO NOT CHECK THIS IN
            return Task.FromResult(true);
            // throw new NotImplementedException();
        }
    }
}
