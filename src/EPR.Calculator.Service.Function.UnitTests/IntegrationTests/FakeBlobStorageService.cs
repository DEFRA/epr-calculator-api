using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.IntegrationTests;

public class FakeBlobStorageService : IStorageService
{
    private readonly Dictionary<string, string> store = new();

    public Task<string> UploadFileContentAsync(
        (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args, CancellationToken cancellationToken = default)
    {
        store[args.FileName] = args.Content;
        return Task.FromResult(args.FileName);
    }

    public string Get(string fileName)
    {
        return store.TryGetValue(fileName, out var content)
            ? content
            : throw new Exception($"Blob not found: {fileName}");
    }

    public void Reset() => store.Clear();
}