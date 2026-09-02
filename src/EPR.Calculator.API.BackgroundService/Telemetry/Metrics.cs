using Duration = System.Diagnostics.Metrics.Histogram<double>;

namespace EPR.Calculator.API.BackgroundService.Telemetry;

/// <summary>
///     This defines the OpenTelemetry metrics used within the background service.
/// </summary>
/// <remarks>
///     Note that this is referenced directly by <see cref="ActivityMetricAttribute" /> to resolve supplied metric names.
/// </remarks>
internal static class Metrics
{
    internal static readonly Duration TotalDuration     = New("run.total_duration",     "The total duration of the run (includes all other activities).");
    internal static readonly Duration PomStreamDelay    = New("run.pom_stream_delay",   "The delay before the POM stream became available.");
    internal static readonly Duration OrgStreamDelay    = New("run.org_stream_delay",   "The delay before the ORG stream became available.");
    internal static readonly Duration DataDuration      = New("run.data_duration",      "The duration of all the data transpose operations (excludes streaming).");
    internal static readonly Duration CalcDuration      = New("run.calc_duration",      "The duration of all the calculation operations.");
    internal static readonly Duration SerializeDuration = New("run.serialize_duration", "The duration of all the serialization operations.");

    private static Duration New(string metricName, string description)
        => Internals.Telemetry.Meter.CreateHistogram<double>(metricName, "ms", description);
}
