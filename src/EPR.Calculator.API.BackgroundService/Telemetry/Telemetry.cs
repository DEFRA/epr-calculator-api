using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Telemetry.Helpers;
using EPR.Calculator.API.BackgroundService.Utils;

namespace EPR.Calculator.API.BackgroundService.Telemetry;

/// <summary>
///     Traces units of work as an OpenTelemetry <see cref="System.Diagnostics.Activity">Activities</see> and
///     <see cref="System.Diagnostics.Metrics.Meter">Metrics</see>.
/// </summary>
/// <remarks>
///     Prefer injecting <see cref="ITelemetry{TCategory}" /> so activities are categorised by the requesting type.
/// </remarks>
public interface ITelemetry
{
    IDisposable BeginScope(TelemetryScope scope);

    void Activity(Action action, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        => Activity(Unit.Wrap(action), threshold, activityName);

    T Activity<T>(Func<T> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "");

    Task Activity(Func<Task> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        => Activity(Unit.Wrap(func), threshold, activityName);

    Task<T> Activity<T>(Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "");

    Task Metric(Histogram<double> histogram, Func<Task> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        => Metric(histogram, Unit.Wrap(func), threshold, activityName);

    Task<T> Metric<T>(Histogram<double> histogram, Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "");
}

/// <summary>
///     An <see cref="ITelemetry" /> whose activities and metrics are categorised under
///     <typeparamref name="TCategory" />'s fully qualified type name.
/// </summary>
/// <typeparam name="TCategory">
///     The type that owns the traced activities - usually the injecting class itself.
/// </typeparam>
[SuppressMessage("SonarAnalyzer.CSharp", "S2326:'TCategory' is not used in the interface", Justification = "Required.")]
public interface ITelemetry<out TCategory> : ITelemetry;

[ExcludeFromCodeCoverage]
public abstract class Telemetry
{
    /// <remarks>
    ///     Unfortunately, this doesn't do anything in Azure - AzureMonitor apparently hardcodes the namespace as
    ///     'azure.applicationinsights'.
    /// </remarks>
    public const string RootScope = "epr.paycal";

    /// <summary>
    ///     Activity tag carrying the logging category for the type that started the activity
    ///     <see cref="Telemetry{TCategory}" />), so <see cref="Helpers.TelemetryLoggingHost" /> can log under a per-type
    ///     category instead of one fixed category for every activity.
    /// </summary>
    public const string CategoryTag = "category";

    public const string ThresholdTag = "duration_warning_threshold";
    protected static readonly TimeSpan DefaultThreshold = TimeSpan.FromSeconds(10);

    public static readonly ActivitySource ActivitySource = new(RootScope);
    public static readonly Meter Meter = new(RootScope);

    /// <summary>
    ///     The ambient scope established by <see cref="BeginScopeCore" />. Backed by an <see cref="AsyncLocal{T}" /> so
    ///     it flows across async calls. It is declared on this non-generic base so that it is shared across every
    ///     category instead of each category tracking its own independent ambient value.
    /// </summary>
    protected static readonly AsyncLocal<TelemetryScope?> AmbientScope = new();

    /// <summary>
    ///     Establishes <paramref name="scope" /> as the ambient scope of the returned <see cref="IDisposable" />.
    ///     Disposing it restores whatever scope (if any) was active beforehand, so scopes may be safely nested.
    /// </summary>
    protected static IDisposable BeginScopeCore(TelemetryScope scope)
    {
        var previous = AmbientScope.Value;
        AmbientScope.Value = scope;
        return new DisposingScope(previous);
    }

    private sealed class DisposingScope(TelemetryScope? previous) : IDisposable
    {
        public void Dispose() => AmbientScope.Value = previous;
    }
}

/// <inheritdoc cref="ITelemetry{TCategory}" />
[ExcludeFromCodeCoverage]
public sealed class Telemetry<TCategory> : Telemetry, ITelemetry<TCategory>
{
    /// <summary>
    ///     A shared instance for <typeparamref name="TCategory" />, used by <see cref="ActivityMetricAttribute" /> when
    ///     the target type has no <see cref="ITelemetry" /> constructor parameter.
    /// </summary>
    internal static readonly Telemetry<TCategory> Instance = new();

    /// <summary>
    ///     The fully qualified name of <typeparamref name="TCategory" />, used as the activity's log category.
    /// </summary>
    private static readonly string Category = typeof(TCategory).FullName ?? typeof(TCategory).Name;

    public IDisposable BeginScope(TelemetryScope scope) => BeginScopeCore(scope);

    public T Activity<T>(Func<T> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
    {
        using var activity = StartActivity(activityName, threshold);

        try
        {
            var result = func();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Unset);
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    public async Task<T> Activity<T>(Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
    {
        using var activity = StartActivity(activityName, threshold);

        try
        {
            var result = await func();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Unset);
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    public async Task<T> Metric<T>(Histogram<double> histogram, Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
    {
        // Note that since there is no try/finally here, metrics are only recorded for
        // successful operations.
        var started = Stopwatch.GetTimestamp();
        var result = await Activity(func, threshold, activityName);
        var duration = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        histogram.Record(duration, AmbientScope.Value?.MetricTags ?? []);
        return result;
    }

    private static Activity? StartActivity(string name, TimeSpan? warningThreshold = null)
    {
        // Tags need to be supplied up-front to StartActivity rather than via .SetTag(),
        // otherwise they are only visible to OnEnd, not OnStart.
        KeyValuePair<string, object?>[] tags =
        [
            new (CategoryTag, Category),
            new (ThresholdTag, warningThreshold ?? DefaultThreshold),
            .. AmbientScope.Value?.ActivityTags ?? []
        ];

        var activity = ActivitySource.StartActivity(name, ActivityKind.Internal, default(ActivityContext), tags);

        if (activity is not null && AmbientScope.Value?.Label is { } label)
            activity.DisplayName = $"{name} | {label}";

        return activity;
    }
}
