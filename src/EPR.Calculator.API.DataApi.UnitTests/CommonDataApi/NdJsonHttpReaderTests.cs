using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Api.DataApi.CommonDataApi.Infrastructure;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi;

/// <summary>
///     Unit tests for <see cref="NdJsonHttpReader" />, shared by the organisation/POM stream
///     request handlers.
/// </summary>
[TestClass]
public class NdJsonHttpReaderTests
{
    private sealed record TestRecord(int Id, string? Name);

    [TestMethod]
    public async Task ReadAsync_WithValidNdJson_DeserializesAllRecords()
    {
        var ndJson = string.Join('\n', Enumerable.Range(1, 3)
            .Select(i => JsonSerializer.Serialize(new TestRecord(i, $"Record {i}"))));
        var client = CreateClient(CreateOkHandler(ndJson));

        var results = await CollectAsync(Read(client));

        results.Count.ShouldBe(3);
        results.Select(r => r.Id).ShouldBe([1, 2, 3]);
    }

    [TestMethod]
    public async Task ReadAsync_WithEmptyResponse_ReturnsNoRecords()
    {
        var client = CreateClient(CreateOkHandler(string.Empty));

        var results = await CollectAsync(Read(client));

        results.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task ReadAsync_SkipsBlankLines()
    {
        var record = JsonSerializer.Serialize(new TestRecord(1, "Record 1"));
        var client = CreateClient(CreateOkHandler($"\n{record}\n\n"));

        var results = await CollectAsync(Read(client));

        results.Count.ShouldBe(1);
    }

    [TestMethod]
    public async Task ReadAsync_WhenServerReturnsError_ThrowsHttpRequestException()
    {
        var client = CreateClient(CreateErrorHandler(HttpStatusCode.InternalServerError));

        await Should.ThrowAsync<HttpRequestException>(async () => await CollectAsync(Read(client)));
    }

    [TestMethod]
    public async Task ReadAsync_SendsRequestAsHttp11()
    {
        HttpRequestMessage? capturedRequest = null;
        var client = CreateClient(new RequestCapturingHandler(request =>
        {
            capturedRequest = request;
            return OkNdJson(string.Empty);
        }));

        await CollectAsync(Read(client));

        capturedRequest.ShouldNotBeNull();
        capturedRequest.Version.ShouldBe(HttpVersion.Version11);
    }

    [TestMethod]
    public async Task ReadAsync_WithGzipResponse_DecompressesCorrectly()
    {
        var ndJson = JsonSerializer.Serialize(new TestRecord(1, "Record 1"));
        var client = CreateClient(CreateCompressedHandler(ndJson, "gzip", Compress<GZipStream>));

        var results = await CollectAsync(Read(client));

        results.Count.ShouldBe(1);
        results[0].Id.ShouldBe(1);
    }

    [TestMethod]
    public async Task ReadAsync_WithDeflateResponse_DecompressesCorrectly()
    {
        var ndJson = JsonSerializer.Serialize(new TestRecord(1, "Record 1"));
        var client = CreateClient(CreateCompressedHandler(ndJson, "deflate", Compress<DeflateStream>));

        var results = await CollectAsync(Read(client));

        results.Count.ShouldBe(1);
        results[0].Id.ShouldBe(1);
    }

    private static IAsyncEnumerable<TestRecord> Read(HttpClient client) =>
        NdJsonHttpReader.ReadAsync<TestRecord>(client, "test", TimeSpan.FromSeconds(30));

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://test-api.example.com") };

    private static MockHandler CreateOkHandler(string ndJsonContent) => new(_ => OkNdJson(ndJsonContent));

    private static MockHandler CreateErrorHandler(HttpStatusCode statusCode) => new(_ => new HttpResponseMessage(statusCode));

    private static HttpResponseMessage OkNdJson(string content) =>
        new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/x-ndjson") };

    private static MockHandler CreateCompressedHandler(string ndJsonContent, string encoding, Func<byte[], byte[]> compress) =>
        new(_ =>
        {
            var compressed = compress(Encoding.UTF8.GetBytes(ndJsonContent));
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(compressed) };
            response.Content.Headers.ContentEncoding.Add(encoding);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-ndjson");
            return response;
        });

    private static byte[] Compress<TStream>(byte[] data) where TStream : Stream
    {
        using var output = new MemoryStream();
        using (var compressor = (Stream)Activator.CreateInstance(typeof(TStream), output, CompressionMode.Compress)!)
        {
            compressor.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> stream)
    {
        var list = new List<T>();
        await foreach (var item in stream)
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>A mock HTTP message handler that delegates response creation to a factory function.</summary>
    private sealed class MockHandler(Func<string, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request.RequestUri?.PathAndQuery ?? string.Empty));
    }

    /// <summary>A mock HTTP message handler that exposes the full request to the factory function.</summary>
    private sealed class RequestCapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }
}
