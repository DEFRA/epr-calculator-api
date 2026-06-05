using EPR.Calculator.Service.Function.Services;

public class FakeBlobStorageService : IStorageUploadService
{
    private readonly Dictionary<string, string> store = new();

    public Task<string> UploadFileContentAsync(
        (string FileName, string Content, string RunName, string ContainerName, bool Overwrite) args)
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