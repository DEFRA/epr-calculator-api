using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.Telemetry;

/// <summary>
///     Abstraction over <see cref="Microsoft.ApplicationInsights.TelemetryClient" />.
/// </summary>
public interface ITelemetryClient
{
    /// <summary>
    ///     Track a telemetry event.
    /// </summary>
    /// <remarks>
    ///     Note that events are generally used for business purposes (triggers, etc.) rather than metrics/logging.
    /// </remarks>
    void TrackEvent(EventTelemetry telemetry);

    /// <summary>
    ///     Records a duration as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    /// <param name="metric">
    ///     The metric to record. This should be in <c>PascalCase</c> and end with <c>Ms</c>. It will be suffixed automatically
    ///     with <c>DurationMs</c> if <c>Ms</c> is missing.
    /// </param>
    /// <param name="duration">The duration to record for the metric.</param>
    void TrackDuration(string metric, TimeSpan duration);

    /// <summary>
    ///     Runs the supplied action and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    /// <param name="metric">
    ///     The metric to record. This should be in <c>PascalCase</c> and end with <c>Ms</c>. It will be suffixed automatically
    ///     with <c>DurationMs</c> if <c>Ms</c> is missing.
    /// </param>
    /// <param name="action">The synchronous action to invoke.</param>
    void TrackDuration(string metric, Action action);

    /// <summary>
    ///     Runs the supplied func, returning its result, and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    /// <param name="metric">
    ///     The metric to record. This should be in <c>PascalCase</c> and end with <c>Ms</c>. It will be suffixed automatically
    ///     with <c>DurationMs</c> if <c>Ms</c> is missing.
    /// </param>
    /// <param name="func">The synchronous function to invoke.</param>
    T TrackDuration<T>(string metric, Func<T> func);

    /// <summary>
    ///     Runs the supplied async action and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    /// <param name="metric">
    ///     The metric to record. This should be in <c>PascalCase</c> and end with <c>Ms</c>. It will be suffixed automatically
    ///     with <c>DurationMs</c> if <c>Ms</c> is missing.
    /// </param>
    /// <param name="asyncAction">The asynchronous action to invoke.</param>
    Task TrackDuration(string metric, Func<Task> asyncAction);

    /// <summary>
    ///     Runs the supplied async func, returning its result, and measures the duration it takes to complete.
    ///     The duration is recorded as a telemetry metric for the input <c>metricId</c>.
    /// </summary>
    /// <param name="metric">
    ///     The metric to record. This should be in <c>PascalCase</c> and end with <c>Ms</c>. It will be suffixed automatically
    ///     with <c>DurationMs</c> if <c>Ms</c> is missing.
    /// </param>
    /// <param name="asyncFunc">The asynchronous function to invoke.</param>
    Task<T> TrackDuration<T>(string metric, Func<Task<T>> asyncFunc);
}
