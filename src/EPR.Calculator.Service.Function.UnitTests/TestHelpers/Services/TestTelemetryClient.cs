using EPR.Calculator.Service.Function.Telemetry;
using Microsoft.ApplicationInsights.DataContracts;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class TestTelemetryClient : ITelemetryClient
{
    private readonly Dictionary<string, EventTelemetry> trackedEvents = [];
    private readonly Dictionary<string, TimeSpan> trackedMetrics = [];

    public virtual void TrackEvent(EventTelemetry telemetry)
        => trackedEvents.Add(telemetry.Name, telemetry);

    public virtual void TrackDuration(string metric, TimeSpan duration)
        => trackedMetrics.Add(metric, duration);

    public void TrackDuration(string metric, Action action)
        => action();

    public virtual T TrackDuration<T>(string metric, Func<T> func)
        => func();

    public virtual async Task TrackDuration(string metric, Func<Task> asyncAction)
        => await asyncAction();

    public virtual async Task<T> TrackDuration<T>(string metric, Func<Task<T>> asyncFunc)
        => await asyncFunc();

    public void VerifyEventTracked(string eventName)
    {
        trackedEvents.ShouldContainKey(eventName);
        trackedEvents[eventName].Properties.Count.ShouldBe(0);
    }

    public void VerifyEventTracked(string eventName, IReadOnlyDictionary<string, string> expectedProperties)
    {
        trackedEvents.ShouldContainKey(eventName);
        trackedEvents[eventName].Properties.ShouldBeEquivalentTo(expectedProperties);
    }

    public void VerifyMetricTracked(string metricId, TimeSpan duration)
    {
        trackedMetrics.ShouldContainKey(metricId);
        trackedMetrics[metricId].ShouldBe(duration);
    }
}
