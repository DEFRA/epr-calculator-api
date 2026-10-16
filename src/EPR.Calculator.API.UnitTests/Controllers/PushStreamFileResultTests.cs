using System.IO.Compression;
using System.Text;
using EPR.Calculator.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class PushStreamFileResultTests
{
    [TestMethod]
    public async Task ExecuteResultAsync_WritesHeadersAndStreamsContentToTheResponseBody()
    {
        var content = Encoding.UTF8.GetBytes("a,b,c");
        var httpContext = new DefaultHttpContext();
        using var body = new MemoryStream();
        httpContext.Response.Body = body;

        var result = new PushStreamFileResult("text/csv", "Run 1 Résults.csv", (stream, ct) => stream.WriteAsync(content, ct).AsTask());

        await result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        httpContext.Response.ContentType.ShouldBe("text/csv");
        httpContext.Response.Headers.ContentDisposition.ToString()
            .ShouldBe("attachment; filename=\"Run 1 R_sults.csv\"; filename*=UTF-8''Run%201%20R%C3%A9sults.csv");
        body.ToArray().ShouldBe(content);
    }

    [TestMethod]
    [DataRow("br", "br")]
    [DataRow("gzip, deflate, br, zstd", "br")]
    [DataRow("gzip", "gzip")]
    [DataRow("br;q=0, gzip", "gzip")]
    public async Task ExecuteResultAsync_CompressesWithTheBestAcceptedEncoding(string acceptEncoding, string expectedEncoding)
    {
        var content = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("producer,1.000,2.000\r\n", 1000)));
        var (httpContext, body) = CreateHttpContext(acceptEncoding);

        await CreateResult(content).ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        httpContext.Response.Headers.ContentEncoding.ToString().ShouldBe(expectedEncoding);
        httpContext.Response.Headers.Vary.ToString().ShouldBe("Accept-Encoding");
        body.Length.ShouldBeLessThan(content.Length);
        Decompress(body.ToArray(), expectedEncoding).ShouldBe(content);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("deflate")]
    [DataRow("br;q=0, gzip;q=0")]
    public async Task ExecuteResultAsync_SendsUncompressed_WhenNoSupportedEncodingIsAccepted(string? acceptEncoding)
    {
        var content = Encoding.UTF8.GetBytes("a,b,c");
        var (httpContext, body) = CreateHttpContext(acceptEncoding);

        await CreateResult(content).ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        httpContext.Response.Headers.ContentEncoding.ToString().ShouldBeEmpty();
        body.ToArray().ShouldBe(content);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("br")]
    [DataRow("gzip")]
    public async Task ExecuteResultAsync_OnlyWritesAsynchronously(string? acceptEncoding)
    {
        // Kestrel rejects synchronous writes to the response body, so a sync write or flush anywhere in the chain would fail in production.
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new AsyncOnlyStream();
        if (acceptEncoding is not null)
            httpContext.Request.Headers.AcceptEncoding = acceptEncoding;

        var result = new PushStreamFileResult("text/csv", "results.csv", async (stream, ct) =>
        {
            await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), bufferSize: 64 * 1024, leaveOpen: true);
            foreach (var _ in Enumerable.Range(0, 5000))
                await writer.WriteAsync("producer,1.000,2.000\r\n");
            await writer.FlushAsync(ct);
        });

        await Should.NotThrowAsync(() => result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor())));
    }

    private static PushStreamFileResult CreateResult(byte[] content) =>
        new("text/csv", "results.csv", (stream, ct) => stream.WriteAsync(content, ct).AsTask());

    private static (DefaultHttpContext HttpContext, MemoryStream Body) CreateHttpContext(string? acceptEncoding)
    {
        var httpContext = new DefaultHttpContext();
        var body = new MemoryStream();
        httpContext.Response.Body = body;

        if (acceptEncoding is not null)
            httpContext.Request.Headers.AcceptEncoding = acceptEncoding;

        return (httpContext, body);
    }

    private static byte[] Decompress(byte[] compressed, string encoding)
    {
        using var input = new MemoryStream(compressed);
        using Stream decompressor = encoding == "br"
            ? new BrotliStream(input, CompressionMode.Decompress)
            : new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        decompressor.CopyTo(output);
        return output.ToArray();
    }

    private sealed class AsyncOnlyStream : MemoryStream
    {
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous write");

        public override void Write(ReadOnlySpan<byte> buffer) => throw new InvalidOperationException("Synchronous write");

        public override void Flush() => throw new InvalidOperationException("Synchronous flush");

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            base.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            buffer.Span.CopyTo(GetSpanForWrite(buffer.Length));
            return ValueTask.CompletedTask;
        }

        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private Span<byte> GetSpanForWrite(int length)
        {
            var position = (int)Position;
            SetLength(Math.Max(Length, position + length));
            Position = position + length;
            return GetBuffer().AsSpan(position, length);
        }
    }
}
