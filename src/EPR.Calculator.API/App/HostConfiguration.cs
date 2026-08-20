using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.Extensions;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace EPR.Calculator.API.App;

[ExcludeFromCodeCoverage]
public static class HostConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder ConfigurePayCalLogging()
        {
            // Clear default logging providers to avoid duplicate logging with Serilog.
            // Note that Serilog must still be called with `writeToProviders: true` so that it
            // can write to other providers added afterwards, such as AppInsights telemetry.
            builder.Logging.ClearProviders();

            builder.Host.UseSerilog((ctx, services, logger) =>
            {
                var telemetrySource = Matching.FromSource(typeof(LoggerTelemetryClient).FullName!);

                logger.ReadFrom.Configuration(ctx.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();

                if (ctx.HostingEnvironment.IsLocal())
                {
                    // Use human readable DevConsole for local envs
                    logger
                        .WriteTo.Logger(lc => lc
                            .Filter.ByExcluding(telemetrySource)
                            .WriteTo.Console(DevConsole.Logger()))
                        .WriteTo.Logger(lc => lc
                            .Filter.ByIncludingOnly(telemetrySource)
                            .MinimumLevel.Verbose()
                            .WriteTo.Console(DevConsole.Telemetry()));
                }
                else
                {
                    // Other envs should still log to console (for live log sessions, etc)
                    logger.WriteTo.Console(new RenderedCompactJsonFormatter());
                }
            }, writeToProviders: true);

            return builder;
        }
    }
}
