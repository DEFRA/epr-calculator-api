using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Logging;

/// <summary>
///     Telemetry client that forwards calls to logger instead of AppInsights if telemetry is disabled.
/// </summary>
[ExcludeFromCodeCoverage]
public class LoggerTelemetryClient(ILogger<LoggerTelemetryClient> logger)
    : ITelemetryClient
{
    // LogLevel.Information since telemetry should be more important than basic logging
    private const LogLevel DefaultLogLevel = LogLevel.Information;

    /// <inheritdoc />
    public void TrackEvent(EventTelemetry telemetry)
    {
        if (telemetry.Properties.Count > 0)
        {
            logger.Log(DefaultLogLevel, "Event {EventName} - {Properties}",
                telemetry.Name, string.Join(", ", telemetry.Properties.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }
        else
        {
            logger.Log(DefaultLogLevel, "Event {EventName}",
                telemetry.Name);
        }
    }

    /// <inheritdoc />
    public void TrackDuration(string metric, TimeSpan duration)
    {
        metric = AppInsightsTelemetryClient.MakeCanonical(metric);

        logger.Log(DefaultLogLevel, "Metric {MetricId}={DurationMs}",
            metric, (int)duration.TotalMilliseconds);
    }

    /// <inheritdoc />
    public void TrackDuration(string metric, Action action)
    {
        metric = AppInsightsTelemetryClient.MakeCanonical(metric);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(DefaultLogLevel, "Measuring {MetricId}...", metric);
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
        metric = AppInsightsTelemetryClient.MakeCanonical(metric);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(DefaultLogLevel, "Measuring {MetricId}...", metric);
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
        metric = AppInsightsTelemetryClient.MakeCanonical(metric);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(DefaultLogLevel, "Measuring {MetricId}...", metric);
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
        metric = AppInsightsTelemetryClient.MakeCanonical(metric);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(DefaultLogLevel, "Measuring {MetricId}...", metric);
            return await asyncFunc();
        }
        finally
        {
            TrackDuration(metric, Stopwatch.GetElapsedTime(startTime));
        }
    }
}
