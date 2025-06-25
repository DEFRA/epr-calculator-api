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
            if (string.IsNullOrWhiteSpace(blobUri))
            {
                return Task.FromResult(Results.NotFound("No file path provided."));
            }

            // Normalize and check if blobUri already ends with fileName
            var normalizedBlobUri = blobUri.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            var normalizedFileName = fileName.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            string filePath = normalizedBlobUri.EndsWith(normalizedFileName, StringComparison.OrdinalIgnoreCase)
                ? normalizedBlobUri
                : Path.Combine(normalizedBlobUri, normalizedFileName);

            if (!File.Exists(filePath))
            {
                return Task.FromResult(Results.NotFound($"File not found at path: {filePath}"));
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var contentType = "application/octet-stream";
            var downloadFileName = Path.GetFileName(filePath);

            return Task.FromResult(Results.File(fileStream, contentType, downloadFileName));
        }

        public Task<bool> IsBlobExistsAsync(string fileName, string blobUri, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
            // throw new NotImplementedException();
        }
    }
}
