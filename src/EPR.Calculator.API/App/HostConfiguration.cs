using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Extensions;
using Serilog;
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
                logger.ReadFrom.Configuration(ctx.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();

                if (ctx.HostingEnvironment.IsLocal())
                    // DevConsole provides nice debugging experience when running locally
                    logger.WriteTo.LocalDevConsole();
                else
                    // Other envs should log to console in an appropriate format (for live log sessions, etc.)
                    logger.WriteTo.Console(new RenderedCompactJsonFormatter());
            }, writeToProviders: true);

            return builder;
        }
    }
}
