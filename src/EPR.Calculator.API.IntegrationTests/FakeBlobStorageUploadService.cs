using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.IntegrationTests;

public class FakeBlobStorageUploadService : IStorageUploadService
{
    private readonly Dictionary<string, string> store = new();

    public Task<string> UploadFileContentAsync(IStorageUploadService.Request request, CancellationToken cancellationToken)
    {
        store[request.FileName] = request.Content;
        return Task.FromResult(request.FileName);
    }

    public string Get(string fileName)
    {
        return store.TryGetValue(fileName, out var content)
            ? content
            : throw new Exception($"Blob not found: {fileName}");
    }

    public void Reset() => store.Clear();
}
