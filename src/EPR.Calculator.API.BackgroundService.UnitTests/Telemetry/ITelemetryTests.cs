using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Telemetry;

/// <summary>
///     Unit tests for <see cref="ITelemetry" />'s default interface method implementations - the
///     <see cref="Action" />/<see cref="Task" />-returning overloads that adapt onto the required generic
///     <c>Activity</c>/<c>Metric</c> members.
/// </summary>
/// <remarks>
///     A hand-written fake stands in for <see cref="ITelemetry" /> rather than <c>Mock&lt;ITelemetry&gt;</c>:
///     Moq intercepts every interface member, including ones with a default body, so an auto-mock would never
///     actually execute the logic under test here (see <c>TelemetryCustomization</c>). The fake is also
///     accessed through an <see cref="ITelemetry" />-typed variable throughout: default interface methods are
///     only reachable via an interface-typed reference, not via the implementing type directly.
/// </remarks>
[TestClass]
public class ITelemetryTests
{
    // ─────────────────────────── Activity(Action) ───────────────────────────

    [TestMethod]
    public void Activity_WithAction_InvokesActionExactlyOnce()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        var invocationCount = 0;
        Action action = () => invocationCount++;

        // Act
        telemetry.Activity(action);

        // Assert
        invocationCount.ShouldBe(1);
    }

    [TestMethod]
    public void Activity_WithAction_DefaultsActivityNameToCallingMember()
    {
        // Arrange
        var (telemetry, recorder) = CreateTelemetry();
        Action action = () => { };

        // Act
        telemetry.Activity(action);

        // Assert: [CallerMemberName] resolves at this call site and must be threaded through to the
        // underlying Activity<T> call rather than defaulting to e.g. the interface method's own name.
        recorder.LastActivityName.ShouldBe(nameof(Activity_WithAction_DefaultsActivityNameToCallingMember));
    }

    [TestMethod]
    public void Activity_WithAction_HonoursExplicitNameAndThreshold()
    {
        // Arrange
        var (telemetry, recorder) = CreateTelemetry();
        Action action = () => { };
        var threshold = TimeSpan.FromSeconds(42);

        // Act
        telemetry.Activity(action, threshold, "CustomActivity");

        // Assert
        recorder.LastActivityName.ShouldBe("CustomActivity");
        recorder.LastThreshold.ShouldBe(threshold);
    }

    [TestMethod]
    public void Activity_WithAction_PropagatesExceptionFromAction()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        var expected = new InvalidOperationException("Simulated failure");
        Action action = () => throw expected;

        // Act
        var actual = Should.Throw<InvalidOperationException>(() => telemetry.Activity(action));

        // Assert: telemetry wrapping must never swallow a failure of the traced code.
        actual.ShouldBeSameAs(expected);
    }

    // ─────────────────────────── Activity(Func<Task>) ───────────────────────────

    [TestMethod]
    public async Task Activity_WithAsyncFunc_InvokesFuncExactlyOnce()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        var invocationCount = 0;
        Func<Task> func = () =>
        {
            invocationCount++;
            return Task.CompletedTask;
        };

        // Act
        await telemetry.Activity(func);

        // Assert
        invocationCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task Activity_WithAsyncFunc_DefaultsActivityNameToCallingMember()
    {
        // Arrange
        var (telemetry, recorder) = CreateTelemetry();
        Func<Task> func = () => Task.CompletedTask;

        // Act
        await telemetry.Activity(func);

        // Assert
        recorder.LastActivityName.ShouldBe(nameof(Activity_WithAsyncFunc_DefaultsActivityNameToCallingMember));
    }

    [TestMethod]
    public async Task Activity_WithAsyncFunc_PropagatesExceptionFromFunc()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        var expected = new InvalidOperationException("Simulated failure");
        Func<Task> func = () => throw expected;

        // Act
        var actual = await Should.ThrowAsync<InvalidOperationException>(async () => await telemetry.Activity(func));

        // Assert
        actual.ShouldBeSameAs(expected);
    }

    // ─────────────────────────── Metric(Histogram, Func<Task>) ───────────────────────────

    [TestMethod]
    public async Task Metric_WithAsyncFunc_InvokesFuncExactlyOnce()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        using var meter = new Meter(nameof(ITelemetryTests));
        var histogram = meter.CreateHistogram<double>("test-metric");
        var invocationCount = 0;
        Func<Task> func = () =>
        {
            invocationCount++;
            return Task.CompletedTask;
        };

        // Act
        await telemetry.Metric(histogram, func);

        // Assert
        invocationCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task Metric_WithAsyncFunc_ForwardsHistogramNameAndThreshold()
    {
        // Arrange
        var (telemetry, recorder) = CreateTelemetry();
        using var meter = new Meter(nameof(ITelemetryTests));
        var histogram = meter.CreateHistogram<double>("test-metric");
        var threshold = TimeSpan.FromSeconds(7);
        Func<Task> func = () => Task.CompletedTask;

        // Act
        await telemetry.Metric(histogram, func, threshold, "CustomMetric");

        // Assert: a bug here would silently record duration against the wrong metric.
        recorder.LastHistogram.ShouldBeSameAs(histogram);
        recorder.LastActivityName.ShouldBe("CustomMetric");
        recorder.LastThreshold.ShouldBe(threshold);
    }

    [TestMethod]
    public async Task Metric_WithAsyncFunc_PropagatesExceptionFromFunc()
    {
        // Arrange
        var (telemetry, _) = CreateTelemetry();
        using var meter = new Meter(nameof(ITelemetryTests));
        var histogram = meter.CreateHistogram<double>("test-metric");
        var expected = new InvalidOperationException("Simulated failure");
        Func<Task> func = () => throw expected;

        // Act
        var actual = await Should.ThrowAsync<InvalidOperationException>(async () => await telemetry.Metric(histogram, func));

        // Assert
        actual.ShouldBeSameAs(expected);
    }

    // ─────────────────────────── Helpers ───────────────────────────

    private static (ITelemetry telemetry, RecordingTelemetry recorder) CreateTelemetry()
    {
        var recorder = new RecordingTelemetry();
        return (recorder, recorder);
    }

    /// <summary>
    ///     Records how it was invoked so <see cref="ITelemetry" />'s default method implementations can be
    ///     verified in isolation, without depending on <c>Telemetry&lt;TCategory&gt;</c> or live OpenTelemetry
    ///     listeners.
    /// </summary>
    private sealed class RecordingTelemetry : ITelemetry
    {
        public string? LastActivityName { get; private set; }

        public TimeSpan? LastThreshold { get; private set; }

        public Histogram<double>? LastHistogram { get; private set; }

        public IDisposable BeginScope(TelemetryScope scope) => throw new NotSupportedException("Not exercised by these tests.");

        public T Activity<T>(Func<T> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        {
            LastActivityName = activityName;
            LastThreshold = threshold;
            return func();
        }

        public async Task<T> Activity<T>(Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        {
            LastActivityName = activityName;
            LastThreshold = threshold;
            return await func();
        }

        public async Task<T> Metric<T>(Histogram<double> histogram, Func<Task<T>> func, TimeSpan? threshold = null, [CallerMemberName] string activityName = "")
        {
            LastHistogram = histogram;
            LastActivityName = activityName;
            LastThreshold = threshold;
            return await func();
        }
    }
}
