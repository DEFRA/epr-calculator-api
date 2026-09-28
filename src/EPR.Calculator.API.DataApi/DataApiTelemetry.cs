using System.Diagnostics;

namespace EPR.CommonDataService.DataApi;

/// <summary>
///     Reports each DataApi step as an activity on the "epr.paycal" source, which both the OpenTelemetry
///     exporter and the background service's local TelemetryLoggingHost already listen to by name. The tag
///     keys mirror EPR.Calculator.API.BackgroundService's Telemetry, which this project cannot reference.
/// </summary>
internal static class DataApiTelemetry
{
    private const string SourceName = "epr.paycal";
    private const string CategoryTag = "category";
    private const string ThresholdTag = "duration_warning_threshold";
    private static readonly TimeSpan DefaultThreshold = TimeSpan.FromSeconds(10);
    private static readonly ActivitySource Source = new(SourceName);

    public static Activity? StartActivity(Type owner, string name) =>
        Source.StartActivity(name, ActivityKind.Internal, default(ActivityContext),
        [
            new KeyValuePair<string, object?>(CategoryTag, owner.FullName),
            new KeyValuePair<string, object?>(ThresholdTag, DefaultThreshold),
        ]);

    public static T Trace<T>(Type owner, string name, Func<T> func)
    {
        using var activity = StartActivity(owner, name);

        try
        {
            var result = func();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
