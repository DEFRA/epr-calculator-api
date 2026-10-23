using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
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
            // Shows scalar UI for local/dev environments.
            if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
            {
                app.MapOpenApi().AllowAnonymous();
                app.MapScalarApiReference().AllowAnonymous();
            }

            return app;
        }

        public WebApplication UsePayCalHealthChecks()
        {
            var opts = new HealthCheckOptions { AllowCachingResponses = false };
            app.MapHealthChecks(HealthCheckPath, opts).AllowAnonymous();

            return app;
        }

        // Otherwise the first request after a start pays for the database token, connection pool, EF model and token-validation config.
        public async Task WarmUpAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await using (var scope = app.Services.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                    await dbContext.CalculatorRuns.AsNoTracking().AnyAsync();
                }

                var jwtBearerOptions = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
                if (jwtBearerOptions.ConfigurationManager is not null)
                    await jwtBearerOptions.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);

                app.Logger.LogInformation("Warmed up in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Warm-up failed after {ElapsedMilliseconds} ms, so the first requests will be slower", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
