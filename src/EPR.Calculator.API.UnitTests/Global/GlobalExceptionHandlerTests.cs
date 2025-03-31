using System.Runtime.ExceptionServices;
using System.Text.Json;
using EPR.Calculator.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Global
{
    [TestClass]
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> mockLogger;
        private readonly Mock<IHostEnvironment> mockEnv;
        private readonly GlobalExceptionHandler exceptionHandler;
        private readonly DefaultHttpContext httpContext;
        private readonly CancellationToken cancellationToken;

        public GlobalExceptionHandlerTests()
        {
            this.mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
            this.mockEnv = new Mock<IHostEnvironment>();
            this.exceptionHandler = new GlobalExceptionHandler(this.mockLogger.Object, this.mockEnv.Object);
            this.httpContext = new DefaultHttpContext();
            this.httpContext.Response.Body = new MemoryStream();
            this.cancellationToken = CancellationToken.None;
        }

        public static Exception CreateExceptionWithDummyStackTrace(string message, string dummyStackTrace)
        {
            try
            {
                throw new InvalidOperationException(message);
            }
            catch (Exception ex)
            {
                var edi = ExceptionDispatchInfo.Capture(ex);
                return edi.SourceException;
            }
        }

        [TestMethod]
        public async Task TryHandleAsync_LogsException()
        {
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.mockLogger.Verify(
               logger => logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("An unhandled exception occurred.")),
                   exception,
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        [TestMethod]
        public async Task TryHandleAsync_SetsInternalServerErrorAndJsonContentType()
        {
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            Assert.AreEqual("application/json", this.httpContext.Response.ContentType);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, this.httpContext.Response.StatusCode);
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_InDevelopment()
        {
            this.mockEnv.Setup(env => env.EnvironmentName).Returns(Environments.Development);
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_InLocal()
        {
            this.mockEnv.Setup(env => env.EnvironmentName).Returns("local");
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_DummyStackTrace_InDevelopment()
        {
            this.mockEnv.Setup(env => env.EnvironmentName).Returns(Environments.Development);
            var dummyStackTrace = "at DummyNamespace.DummyClass.DummyMethod() in /DummyFile.cs:line 42";
            var exception = CreateExceptionWithDummyStackTrace("Test exception with dummy stack trace", dummyStackTrace);

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
            Assert.AreEqual(exception.StackTrace, responseObject.GetProperty("Detail").GetString());
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_DummyStackTrace_InLocal()
        {
            this.mockEnv.Setup(env => env.EnvironmentName).Returns("local");
            var dummyStackTrace = "at DummyNamespace.DummyClass.DummyMethod() in /DummyFile.cs:line 42";
            var exception = CreateExceptionWithDummyStackTrace("Test exception with dummy stack trace", dummyStackTrace);

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
            Assert.AreEqual(exception.StackTrace, responseObject.GetProperty("Detail").GetString());
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_NotInDevelopment()
        {
            this.mockEnv.Setup(env => env.EnvironmentName).Returns(Environments.Production);
            var exception = new Exception("Test exception");

            await this.exceptionHandler.TryHandleAsync(this.httpContext, exception, this.cancellationToken);

            this.httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(this.httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
            Assert.IsFalse(responseObject.TryGetProperty("Detail", out _));
        }
    }
}
