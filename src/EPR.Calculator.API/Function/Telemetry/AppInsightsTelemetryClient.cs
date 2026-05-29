using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Metrics;

namespace EPR.Calculator.Service.Function.Telemetry;

[ExcludeFromCodeCoverage]
public class AppInsightsTelemetryClient(TelemetryClient client)
    : ITelemetryClient
{
    private const string MetricNamespace = "epr.paycal";

    /// <inheritdoc />
    public void TrackEvent(EventTelemetry telemetry)
        => client.TrackEvent(telemetry);

    /// <inheritdoc />
    public void TrackDuration(string metric, TimeSpan duration)
    {
        metric = MakeCanonical(metric);
        var metricIdentifier = new MetricIdentifier(MetricNamespace, metric);

        client
            .GetMetric(metricIdentifier)
            .TrackValue((int)duration.TotalMilliseconds);
    }

    /// <inheritdoc />
    public void TrackDuration(string metric, Action action)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            action();
        }
        finally
        {
            TrackDuration(metric, Stopwatch.GetElapsedTime(startTime));
        }
    }

    /// <inheritdoc />
    public T TrackDuration<T>(string metric, Func<T> func)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            return func();
        }
        finally
        {
            TrackDuration(metric, Stopwatch.GetElapsedTime(startTime));
        }
    }

    /// <inheritdoc />
    public async Task TrackDuration(string metric, Func<Task> asyncAction)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            await asyncAction();
        }
        finally
        {
            TrackDuration(metric, Stopwatch.GetElapsedTime(startTime));
        }
    }

    /// <inheritdoc />
    public async Task<T> TrackDuration<T>(string metric, Func<Task<T>> asyncFunc)
    {
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            return await asyncFunc();
        }
        finally
        {
            TrackDuration(metric, Stopwatch.GetElapsedTime(startTime));
        }
    }

    public static string MakeCanonical(string metric)
    {
        // Enforce canonical suffix
        if (!metric.EndsWith("Ms"))
            metric += "DurationMs";

        return metric;
    }
}
