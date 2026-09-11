using System.Text;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Services;

namespace EPR.Calculator.API.IntegrationTests;

public class FakeBlobStorageUploadService : IStorageUploadService, IBlobStorageService
{
    private readonly Dictionary<string, string> store = new();

    public Task<string> UploadFileContentAsync(IStorageUploadService.Request request, CancellationToken cancellationToken)
    {
        store[request.FileName] = request.Content;
        return Task.FromResult(request.FileName);
    }

    public async Task<string> UploadFileStreamAsync(IStorageUploadService.StreamRequest request, Func<Stream, CancellationToken, Task> writeContent, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await writeContent(buffer, cancellationToken);
        store[request.FileName] = Encoding.UTF8.GetString(buffer.ToArray());
        return request.FileName;
    }

    public string Get(string fileName)
    {
        return store.TryGetValue(fileName, out var content)
            ? content
            : throw new Exception($"Blob not found: {fileName}");
    }

    public void Reset() => store.Clear();

    public Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenStream(filename);

    public Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenStream(filename);

    public Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default) =>
        Task.FromResult(store.ContainsKey(filename));

    private Task<Stream?> OpenStream(string filename) =>
        Task.FromResult<Stream?>(store.TryGetValue(filename, out var content)
            ? new MemoryStream(Encoding.UTF8.GetBytes(content))
            : null);
}
