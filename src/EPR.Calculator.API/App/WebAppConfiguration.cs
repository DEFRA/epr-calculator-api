using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;

namespace EPR.Calculator.API.App;

[ExcludeFromCodeCoverage]
public static class WebAppConfiguration
{
    public const string HealthCheckPath = "/admin/health";

    extension(WebApplication app)
    {
        public WebApplication UsePayCalRequestLogging()
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                {
                    if (ex != null || httpContext.Response.StatusCode > 499)
                        return LogEventLevel.Error;

                    // Health check pings are frequent and low-value; demote them to Verbose.
                    return httpContext.Request.Path.StartsWithSegments(HealthCheckPath)
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;
                };
            });

            return app;
        }

        public WebApplication UsePayCalApiExplorer()
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
            {
                app.UseSwagger().UseSwaggerUI();
            }

            return app;
        }

        public WebApplication UsePayCalHealthChecks()
        {
            var opts = new HealthCheckOptions { AllowCachingResponses = false };
            app.MapHealthChecks(HealthCheckPath, opts).AllowAnonymous();

            return app;
        }
    }
}
