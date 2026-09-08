using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace EPR.Calculator.API.Controllers;

// Writes a file response straight to the network stream via a callback, so a large export
// (e.g. the billing JSON) is never buffered whole in memory before it is sent.
public sealed class StreamCallbackResult(string contentType, string fileName, Func<Stream, CancellationToken, Task> writeAsync) : IActionResult
{
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = contentType;
        response.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileName
        }.ToString();

        await writeAsync(response.Body, context.HttpContext.RequestAborted);
    }
}
