using System.Diagnostics;

namespace EPR.Calculator.Api.DataApi;

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
    private const string AllocatedBytesTag = "allocated_bytes";
    private const string HeapBytesTag = "heap_bytes";
    private static readonly TimeSpan DefaultThreshold = TimeSpan.FromSeconds(10);
    private static readonly ActivitySource Source = new(SourceName);

    /// <summary>
    ///     Whether <see cref="AllocatedBytesTag" />/<see cref="HeapBytesTag" /> get captured on every
    ///     activity - set once at startup from config. Off by default: measured expensive under Azure
    ///     Monitor at production activity volumes, so the performance test turns it on instead.
    /// </summary>
    public static bool CaptureMemoryMetrics { get; set; }

    public static Activity? StartActivity(Type owner, string name) =>
        Source.StartActivity(name, ActivityKind.Internal, default(ActivityContext),
        [
            new KeyValuePair<string, object?>(CategoryTag, owner.FullName),
            new KeyValuePair<string, object?>(ThresholdTag, DefaultThreshold),
        ]);

    public static T Trace<T>(Type owner, string name, Func<T> func)
    {
        using var activity = StartActivity(owner, name);
        var allocatedBefore = CaptureMemoryMetrics ? GC.GetTotalAllocatedBytes() : 0;

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
        finally
        {
            if (CaptureMemoryMetrics)
                RecordMemory(activity, allocatedBefore);
        }
    }

    public static Task TraceAsync(Type owner, string name, Func<Task> func) =>
        TraceAsync(owner, name, async () =>
        {
            await func();
            return true;
        });

    public static async Task<T> TraceAsync<T>(Type owner, string name, Func<Task<T>> func)
    {
        using var activity = StartActivity(owner, name);
        var allocatedBefore = CaptureMemoryMetrics ? GC.GetTotalAllocatedBytes() : 0;

        try
        {
            var result = await func();
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
        finally
        {
            if (CaptureMemoryMetrics)
                RecordMemory(activity, allocatedBefore);
        }
    }

    private static void RecordMemory(Activity? activity, long allocatedBefore)
    {
        activity?.SetTag(AllocatedBytesTag, GC.GetTotalAllocatedBytes() - allocatedBefore);
        activity?.SetTag(HeapBytesTag, GC.GetTotalMemory(forceFullCollection: false));
    }
}
