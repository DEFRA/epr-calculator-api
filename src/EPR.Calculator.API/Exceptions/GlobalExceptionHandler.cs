using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;

namespace EPR.Calculator.API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var errorMessage = "An unexpected error occurred.";
            logger.LogError(exception, errorMessage);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                Status = httpContext.Response.StatusCode,
                Message = errorMessage,
                Instance = httpContext.Request.Path
            };

            var errorJson = JsonSerializer.Serialize(errorResponse, Options);
            await httpContext.Response.WriteAsync(errorJson, cancellationToken: cancellationToken);
            return true;
        }
    }
}
