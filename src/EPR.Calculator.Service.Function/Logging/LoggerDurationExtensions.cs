using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EPR.Calculator.Service.Function.Logging;

/// <summary>
///     Extension methods for logging telemetry style metrics using ILogger.
/// </summary>
[ExcludeFromCodeCoverage]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class LoggerDurationExtensions
{
    private const LogLevel DefaultLogLevel = LogLevel.Debug;

    /// <remarks>
    ///     This is intended for diagnostic trace level logging.
    ///     For more important metrics, use the equivalent
    ///     <see cref="ITelemetryClient.TrackDuration(string, TimeSpan)">ITelemetryClient method</see> instead.
    /// </remarks>
    public static void LogDuration(this ILogger logger,
        TimeSpan duration,
        string? metricId = null,
        LogLevel logLevel = DefaultLogLevel,
        [CallerMemberName] string caller = null!)
        => logger.Log(logLevel, @"{MetricId} Duration: {Duration:mm\:ss\.fff}",
            MakeCanonical(metricId, caller), duration);

    /// <remarks>
    ///     This is intended for diagnostic trace level logging.
    ///     For more important metrics, use the equivalent
    ///     <see cref="ITelemetryClient.TrackDuration(string, Action)">ITelemetryClient method</see> instead.
    /// </remarks>
    public static void LogDuration(this ILogger logger,
        Action action,
        string? metricId = null,
        LogLevel logLevel = DefaultLogLevel,
        [CallerMemberName] string caller = null!)
    {
        metricId = MakeCanonical(metricId, caller);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(logLevel, "{MetricId}: Starting...", metricId);
            action();
            logger.Log(logLevel, @"{MetricId}: Completed. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
        }
        catch
        {
            logger.LogError(@"{MetricId}: FAILED. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            throw;
        }
    }

    /// <remarks>
    ///     This is intended for diagnostic trace level logging.
    ///     For more important metrics, use the equivalent
    ///     <see cref="ITelemetryClient.TrackDuration{T}(string, Func{T})">ITelemetryClient method</see> instead.
    /// </remarks>
    public static T LogDuration<T>(this ILogger logger,
        Func<T> action,
        string? metricId = null,
        LogLevel logLevel = DefaultLogLevel,
        [CallerMemberName] string caller = null!)
    {
        metricId = MakeCanonical(metricId, caller);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(logLevel, "{MetricId}: Starting...", metricId);
            var result = action();
            logger.Log(logLevel, @"{MetricId}: Completed. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            return result;
        }
        catch
        {
            logger.LogError(@"{MetricId}: FAILED. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            throw;
        }
    }

    /// <remarks>
    ///     This is intended for diagnostic trace level logging.
    ///     For more important metrics, use the equivalent
    ///     <see cref="ITelemetryClient.TrackDuration(string, Func{Task})">ITelemetryClient method</see> instead.
    /// </remarks>
    public static async Task LogDuration(this ILogger logger,
        Func<Task> action,
        string? metricId = null,
        LogLevel logLevel = DefaultLogLevel,
        [CallerMemberName] string caller = null!)
    {
        metricId = MakeCanonical(metricId, caller);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(logLevel, "{MetricId}: Starting...", metricId);
            await action();
            logger.Log(logLevel, @"{MetricId}: Completed. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
        }
        catch
        {
            logger.LogError(@"{MetricId}: FAILED. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            throw;
        }
    }

    /// <remarks>
    ///     This is intended for diagnostic trace level logging.
    ///     For more important metrics, use the equivalent
    ///     <see cref="ITelemetryClient.TrackDuration{T}(string, Func{Task{T}})">ITelemetryClient method</see> instead.
    /// </remarks>
    public static async Task<T> LogDuration<T>(this ILogger logger,
        Func<Task<T>> action,
        string? metricId = null,
        LogLevel logLevel = DefaultLogLevel,
        [CallerMemberName] string caller = null!)
    {
        metricId = MakeCanonical(metricId, caller);
        var startTime = Stopwatch.GetTimestamp();

        try
        {
            logger.Log(logLevel, "{MetricId}: Starting...", metricId);
            var result = await action();
            logger.Log(logLevel, @"{MetricId}: Completed. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            return result;
        }
        catch
        {
            logger.LogError(@"{MetricId}: FAILED. Duration: {Duration:mm\:ss\.fff}",
                metricId, Stopwatch.GetElapsedTime(startTime));
            throw;
        }
    }

    private static string MakeCanonical(string? metricId, string? caller)
    {
        if (string.IsNullOrWhiteSpace(caller))
            return metricId ?? "Unknown";

        if (string.IsNullOrWhiteSpace(metricId))
            return caller;

        return $"{caller} -> {metricId}";
    }
}
