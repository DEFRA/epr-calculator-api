using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace EPR.Calculator.API.Controllers;

/// <summary>
///     A file download whose content is written straight to the response body as it is generated,
///     rather than built in memory first. Compressed with Brotli or gzip when the client accepts it.
/// </summary>
public sealed class PushStreamFileResult(string contentType, string fileDownloadName, Func<Stream, CancellationToken, Task> writeTo) : IActionResult
{
    public string ContentType => contentType;

    public string FileDownloadName => fileDownloadName;

    public Task WriteTo(Stream stream, CancellationToken cancellationToken) => writeTo(stream, cancellationToken);

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;

        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(fileDownloadName);

        response.ContentType = contentType;
        response.Headers.ContentDisposition = contentDisposition.ToString();
        response.Headers.Vary = HeaderNames.AcceptEncoding;

        var encoding = ChooseEncoding(request.Headers.AcceptEncoding);

        if (encoding is null)
        {
            await WriteTo(response.Body, context.HttpContext.RequestAborted);
            return;
        }

        response.Headers.ContentEncoding = encoding;

        await using Stream compressed = encoding == "br"
            ? new BrotliStream(response.Body, CompressionLevel.Optimal, leaveOpen: true)
            : new GZipStream(response.Body, CompressionLevel.Optimal, leaveOpen: true);

        await WriteTo(compressed, context.HttpContext.RequestAborted);
    }

    private static string? ChooseEncoding(StringValues acceptEncoding)
    {
        if (!StringWithQualityHeaderValue.TryParseList(acceptEncoding, out var accepted))
            return null;

        bool Accepts(string encoding) => accepted.Any(a =>
            a.Value.Equals(encoding, StringComparison.OrdinalIgnoreCase) && a.Quality is not 0);

        if (Accepts("br"))
            return "br";

        return Accepts("gzip") ? "gzip" : null;
    }
}
