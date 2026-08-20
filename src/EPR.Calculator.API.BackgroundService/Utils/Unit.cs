using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.BackgroundService.Utils;

/// <summary>
///     Zero-size placeholder type used to adapt a void-returning delegate (e.g. <see cref="Action" />) to a
///     value-returning one (e.g. <see cref="Func{TResult}" />), so void and non-void call shapes can share one
///     implementation.
/// </summary>
[ExcludeFromCodeCoverage]
internal readonly struct Unit
{
    public static readonly Unit Value = default;

    /// <summary>
    ///     Wraps a void-returning delegate to a value-returning one.
    /// </summary>
    public static Func<Unit> Wrap(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return () =>
        {
            action();
            return Value;
        };
    }

    /// <summary>
    ///     Wraps a void-returning async delegate to a value-returning one.
    /// </summary>
    public static Func<Task<Unit>> Wrap(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        return async () =>
        {
            await func().ConfigureAwait(false);
            return Value;
        };
    }
}
