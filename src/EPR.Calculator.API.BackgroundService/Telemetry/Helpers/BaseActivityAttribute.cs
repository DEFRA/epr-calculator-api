using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace EPR.Calculator.API.BackgroundService.Telemetry.Helpers;

[ExcludeFromCodeCoverage]
internal abstract class BaseActivityAttribute : OverrideMethodAspect
{
    private readonly string? activityName;
    private readonly string? threshold;

    // ReSharper disable once ConvertToPrimaryConstructor - Not supported by Metalama
    protected BaseActivityAttribute(string? activityName, string? threshold)
    {
        this.activityName = activityName;
        this.threshold = threshold;
    }

    [CompileTime] protected string ActivityName => activityName ?? $"{meta.Target.Type.Name}.{meta.Target.Method.Name}";

    [CompileTime] protected TimeSpan? Threshold => threshold != null ? TimeSpan.Parse(threshold, CultureInfo.InvariantCulture) : null;

    /// <summary>
    ///     Returns an expression yielding an <see cref="ITelemetry" /> for <paramref name="type" />: its
    ///     <see cref="ITelemetry" /> constructor parameter, if it has one, or otherwise the shared
    ///     <see cref="Telemetry{TCategory}.Instance" /> categorised under <paramref name="type" /> itself.
    /// </summary>
    protected static IExpression GetTelemetryExpression(INamedType type)
    {
        foreach (var constructor in type.Constructors)
        {
            // If an ITelemetry<> is injected into the constructor, this should return an expression for it.
            var telemetryParam = constructor.Parameters.FirstOrDefault(p => p.Type.IsConvertibleTo(typeof(ITelemetry)));

            if (telemetryParam != null)
                return ExpressionFactory.Parse(telemetryParam.Name);
        }

        // Otherwise, return the shared Telemetry<TCategory>.Instance.
        var telemetryType = TypeFactory.GetNamedType(typeof(Telemetry<>)).MakeGenericInstance([type]);
        return telemetryType.Fields.Single(f => f.Name == nameof(Telemetry<>.Instance));
    }
}
