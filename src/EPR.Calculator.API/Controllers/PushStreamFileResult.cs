using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace EPR.Calculator.API.Controllers;

/// <summary>
///     A file download whose content is written straight to the response body as it is generated,
///     rather than built in memory first.
/// </summary>
public sealed class PushStreamFileResult(string contentType, string fileDownloadName, Func<Stream, CancellationToken, Task> writeTo) : IActionResult
{
    public string ContentType => contentType;

    public string FileDownloadName => fileDownloadName;

    public Task WriteTo(Stream stream, CancellationToken cancellationToken) => writeTo(stream, cancellationToken);

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;

        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(fileDownloadName);

        response.ContentType = contentType;
        response.Headers.ContentDisposition = contentDisposition.ToString();

        await WriteTo(response.Body, context.HttpContext.RequestAborted);
    }
}
