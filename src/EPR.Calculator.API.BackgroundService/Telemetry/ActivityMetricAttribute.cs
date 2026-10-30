using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.BackgroundService.Utils;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace EPR.Calculator.API.BackgroundService.Telemetry;

/// <summary>
///     Calls to the target method will be recorded as an activity trace and have the call duration recorded against the
///     specified metric, as defined in <see cref="Metrics" />.
/// </summary>
/// <remarks>
///     If the target type has an <see cref="ITelemetry" /> constructor parameter, that instance is used; otherwise, a
///     shared <see cref="Telemetry{TCategory}" /> instance categorised under the target type is used instead.
/// </remarks>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Method)]
public sealed class ActivityMetricAttribute : BaseActivityAttribute
{
    private readonly string argMetricName;

    /// <param name="metricName">
    ///     Metric to record against, as defined in <see cref="Metrics" />.
    /// </param>
    /// <param name="activityName">
    ///     The name of the associated activity as shown in traces. Will default to the target method name.
    /// </param>
    /// <param name="threshold">
    ///     A duration threshold to compare against the activity, used to identify slow calls. Will default to <see cref="Telemetry.DefaultThreshold" />.
    /// </param>
    // ReSharper disable once ConvertToPrimaryConstructor - Not supported by Metalama
    public ActivityMetricAttribute(string metricName, string? activityName = null, string? threshold = null)
        : base(activityName, threshold)
    {
        argMetricName = metricName;

        // Metalama only selects OverrideAsyncMethod() for methods with the `async` modifier
        // by default. Methods that return Task/Task<T> by forwarding to another call -
        // without being declared `async` themselves - don't count as async under that rule,
        // even though they are perfectly awaitable. Opting in here routes any awaitable
        // method to OverrideAsyncMethod() below, regardless of the `async` modifier.
        UseAsyncTemplateForAnyAwaitable = true;
    }

    public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
        base.BuildEligibility(builder);

        // Synchronous, non-awaitable methods aren't supported, since ITelemetry has no
        // Metric() overload that can time them. This generates a compile-time error if
        // attempted. Awaitable methods are fine whether or not they're declared `async`
        // themselves - see UseAsyncTemplateForAnyAwaitable above.
        builder.MustSatisfy(
            m => m.GetAsyncInfo().IsAwaitable,
            m => $"{m} must return an awaitable type (e.g. Task or Task<T>), as {nameof(ActivityMetricAttribute)} only supports asynchronous methods");
    }

    // Unreachable in practice: BuildEligibility above rejects non-awaitable methods at
    // compile time, and UseAsyncTemplateForAnyAwaitable routes every awaitable method to
    // OverrideAsyncMethod() below. Kept as a defensive fallback since OverrideMethod() is
    // abstract on the base class.
    public override dynamic OverrideMethod()
        => throw new NotSupportedException($"{nameof(ActivityMetricAttribute)} only supports asynchronous methods.");

    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        Histogram<double> histogram = TypeFactory.GetNamedType(typeof(Metrics)).Fields.Single(f => f.Name == argMetricName).Value!;
        ITelemetry telemetry = GetTelemetryExpression(meta.Target.Type).Value!;
        return await telemetry.Metric(histogram, Proceed, Threshold, ActivityName);

        async Task<object?> Proceed()
        {
            var result = await meta.ProceedAsync();

            // SpecialType.Void means that the target method returns Task rather than Task<T>
            return meta.Target.Method.GetAsyncInfo().ResultType.Equals(SpecialType.Void)
                ? Unit.Value
                : result;
        }
    }
}
