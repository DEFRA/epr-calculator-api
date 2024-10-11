using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                Status = httpContext.Response.StatusCode,
                Title = "An error occurred while processing your request.",
                exception.Message,
                Instance = httpContext.Request.Path,
                Detail = _env.IsDevelopment() ? exception.StackTrace : null
            };

            var errorJson = JsonSerializer.Serialize(errorResponse, options);
            await httpContext.Response.WriteAsync(errorJson, cancellationToken: cancellationToken);
            return true;
        }

        private static readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
