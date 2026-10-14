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
}
