using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
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
