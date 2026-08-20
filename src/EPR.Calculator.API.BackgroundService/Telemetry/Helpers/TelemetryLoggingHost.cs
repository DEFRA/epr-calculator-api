using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;

namespace EPR.Calculator.API.BackgroundService.Telemetry.Helpers;

/// <summary>
///     This is intended to be used for local development. It forwards telemetry activities and metrics to standard
///     loggers (i.e. for console output).
/// </summary>
[ExcludeFromCodeCoverage]
public sealed partial class TelemetryLoggingHost(ILoggerFactory loggerFactory) : IHostedService, IDisposable
{
    // This should start with 'EPR.Calculator' so that it can be filtered via log level overrides.
    public const string MetricCategory = "EPR.Calculator.Metrics";

    private readonly ActivityListener activityListener = new();
    private readonly ConcurrentDictionary<string, ILogger> activityLoggers = new();
    private readonly ILogger hostLogger = loggerFactory.CreateLogger<TelemetryLoggingHost>();
    private readonly MeterListener metricListener = new();
    private readonly ILogger metricLogger = loggerFactory.CreateLogger(MetricCategory);

    public void Dispose()
    {
        activityListener.Dispose();
        metricListener.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        hostLogger.LogInformation("Telemetry logging host started");
        StartActivityListener();
        StartMetricListener();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        hostLogger.LogInformation("Telemetry logging host stopped");
        activityListener.Dispose();
        metricListener.Dispose();

        return Task.CompletedTask;
    }

    private void StartActivityListener()
    {
        activityListener.ShouldListenTo = source => source.Name == Telemetry.RootScope;
        activityListener.Sample = (ref _) => ActivitySamplingResult.AllData;

        activityListener.ActivityStarted = activity =>
        {
            var activityLogger = LoggerFor(activity);

            if (activityLogger is null)
                return;

            var activityName = GetActivityName(activity);
            LogActivityStarting(activityLogger, activityName);
        };

        activityListener.ActivityStopped = activity =>
        {
            var activityLogger = LoggerFor(activity);

            if (activityLogger is null)
                return;

            var activityName = GetActivityName(activity);

            var state = activity.Status switch
            {
                ActivityStatusCode.Ok => "Completed",
                ActivityStatusCode.Error => "Failed",
                _ => "Cancelled"
            };

            var threshold = activity.GetTagItem(Telemetry.ThresholdTag) as TimeSpan?;

            if(threshold > TimeSpan.Zero && activity.Duration > threshold)
                LogWarningActivityEnded(activityLogger, activityName, state, activity.Duration, threshold.Value);
            else
                LogActivityEnded(activityLogger, activityName, state, activity.Duration);
        };

        ActivitySource.AddActivityListener(activityListener);

        ILogger? LoggerFor(Activity activity)
        {
            var category = activity.GetTagItem(Telemetry.CategoryTag) as string;

            return !string.IsNullOrWhiteSpace(category)
                ? activityLoggers.GetOrAdd(category, loggerFactory.CreateLogger)
                : null;
        }

        static string GetActivityName(Activity activity)
        {
            return activity.OperationName.LastIndexOf('.') < 0
                ? activity.OperationName
                : activity.OperationName[(activity.OperationName.LastIndexOf('.') + 1)..];
        }
    }

    private void StartMetricListener()
    {
        metricListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == Telemetry.RootScope)
                listener.EnableMeasurementEvents(instrument);
        };

        metricListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, _)
            => LogMetric(metricLogger, instrument.Name, measurement, instrument.Unit, string.Join(", ", tags.ToArray().Select(tag => $"{tag.Key}={tag.Value}"))));

        metricListener.Start();
    }

    [LoggerMessage(LogLevel.Information, "{Instrument} is {Value}{Unit} [{Tags}]")]
    private static partial void LogMetric(ILogger logger, string instrument, double value, string? unit, string tags);

    [LoggerMessage(LogLevel.Trace, "{Activity}: Starting...")]
    private static partial void LogActivityStarting(ILogger logger, string activity);

    [LoggerMessage(LogLevel.Debug, "{Activity}: {State} after {Duration}")]
    private static partial void LogActivityEnded(ILogger logger, string activity, string state, TimeSpan duration);

    [LoggerMessage(LogLevel.Warning, "{Activity}: {State} after {Duration} (over threshold {Threshold})")]
    private static partial void LogWarningActivityEnded(ILogger logger, string activity, string state, TimeSpan duration, TimeSpan threshold);
}
