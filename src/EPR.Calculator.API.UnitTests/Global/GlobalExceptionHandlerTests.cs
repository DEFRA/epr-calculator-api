using EPR.Calculator.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.API.UnitTests.Global
{
    [TestClass]
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> mockLogger;
        private readonly GlobalExceptionHandler exceptionHandler;
        private readonly DefaultHttpContext httpContext;
        private readonly CancellationToken cancellationToken;

        public GlobalExceptionHandlerTests()
        {
            this.mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
            this.exceptionHandler = new GlobalExceptionHandler(this.mockLogger.Object);
            this.httpContext = new DefaultHttpContext();
            this.httpContext.Response.Body = new MemoryStream();
            this.cancellationToken = CancellationToken.None;
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TryHandleAsync_LogsException_AndReturns500()
        {
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            Assert.AreEqual("application/json", this.httpContext.Response.ContentType);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, this.httpContext.Response.StatusCode);
            var responseBody = this.httpContext.Response.Body;
            responseBody.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual("""{"Status":500,"Message":"An unexpected error occurred","Instance":{"Value":"","HasValue":false}}""", await new StreamReader(responseBody).ReadToEndAsync());

            this.mockLogger.Verify(
               logger => logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("An unexpected error occurred")),
                   exception,
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }
    }
}
