using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Services;

namespace EPR.Calculator.API.IntegrationTests;

public class FakeBlobStorageUploadService : IStorageUploadService, IBlobStorageService, IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("fake-blob-").FullName;

    public async Task<string> UploadFileContentAsync(IStorageUploadService.Request request, CancellationToken cancellationToken)
    {
        await File.WriteAllBytesAsync(PathFor(request.FileName), request.Content, cancellationToken);
        return request.FileName;
    }

    public async Task<string> UploadFileStreamAsync(IStorageUploadService.StreamRequest request, Func<Stream, CancellationToken, Task> writeContent, CancellationToken cancellationToken)
    {
        await using (var file = File.Create(PathFor(request.FileName)))
            await writeContent(file, cancellationToken);
        return request.FileName;
    }

    public byte[] Get(string fileName)
    {
        var path = PathFor(fileName);
        return File.Exists(path) ? File.ReadAllBytes(path) : throw new Exception($"Blob not found: {fileName}");
    }

    // For callers that just want the uploaded content on disk elsewhere (e.g. the performance
    // test saving its output) - copies the file directly, without materialising it as a string.
    public void CopyTo(string fileName, string destinationPath) => File.Copy(PathFor(fileName), destinationPath, overwrite: true);

    public void Reset()
    {
        foreach (var file in Directory.EnumerateFiles(root))
            File.Delete(file);
    }

    public Task<Stream?> OpenResultCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenStream(filename);

    public Task<Stream?> OpenBillingCsvStream(string filename, CancellationToken cancellationToken = default) =>
        OpenStream(filename);

    public Task<Stream?> OpenBillingJsonStream(string filename, CancellationToken cancellationToken = default) =>
        OpenStream(filename);

    public Task<bool> MoveBillingJsonToFss(string filename, CancellationToken cancellationToken = default) =>
        Task.FromResult(File.Exists(PathFor(filename)));

    public void Dispose()
    {
        // Registered under three service types resolving to this same instance, so the container
        // disposes it more than once - guard the delete rather than fail on the second call.
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
        GC.SuppressFinalize(this);
    }

    private Task<Stream?> OpenStream(string filename)
    {
        var path = PathFor(filename);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    private string PathFor(string fileName) => Path.Combine(root, fileName);
}
