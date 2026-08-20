using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Telemetry.Helpers;

namespace EPR.Calculator.API.BackgroundService.Telemetry;

[ExcludeFromCodeCoverage]
internal static class TelemetryExtensions
{
    extension(ILogger logger)
    {
        public IDisposable? BeginRunScope(RunContext runContext)
            => logger.BeginScope(runContext.ActivityTags);
    }

    extension(ITelemetry telemetry)
    {
        public IDisposable BeginRunScope(RunContext runContext)
            => telemetry.BeginScope(new TelemetryScope
            {
                Label = $"RunId: {runContext.RunId}",
                ActivityTags = runContext.ActivityTags,
                MetricTags = runContext.MetricTags
            });
    }

    extension(RunContext runContext)
    {
        /// <summary>
        ///     OpenTelemetry tags for high-cardinality run properties, suitable for activity traces.
        /// </summary>
        private KeyValuePair<string, object?>[] ActivityTags =>
        [
            .. runContext.MetricTags,
            new ("run.id", runContext.RunId),
            new ("run.name", runContext.RunName)
        ];

        /// <summary>
        ///     OpenTelemetry tags for low-cardinality run properties, suitable for metrics.
        /// </summary>
        private KeyValuePair<string, object?>[] MetricTags =>
        [
            new ("run.type", runContext.RunType.ToString()),
            new ("run.relative_year", (int) runContext.RelativeYear)
        ];
    }
}
