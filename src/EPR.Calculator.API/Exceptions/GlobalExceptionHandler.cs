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
        private readonly IHostEnvironment env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            this.logger.LogError(exception, CommonResources.AnUnexpectedErrorOccurred);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                Status = httpContext.Response.StatusCode,
                Title = CommonResources.AnErrorProcessingYourRequest,
                exception.Message,
                Instance = httpContext.Request.Path,
                Detail = this.env.IsDevelopment() || this.env.EnvironmentName?.ToLower() == CommonResources.Local.ToLower() ? exception.StackTrace : null,
            };

            var errorJson = JsonSerializer.Serialize(errorResponse, Options);
            await httpContext.Response.WriteAsync(errorJson, cancellationToken: cancellationToken);
            return true;
        }
    }
}
