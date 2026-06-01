using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Messaging;

/// <summary>
///     Represents the base class for all message types.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract record MessageBase
{
    /// <summary>
    ///     Gets or sets the type identifier of the message.
    ///     This value is typically used to determine the specific message subclass during deserialization.
    /// </summary>
    public required string MessageType { get; init; }

    /// <summary>
    ///     Gets or sets the identifier for the calculator run.
    /// </summary>
    public int CalculatorRunId { get; init; }

    public abstract RunType RunType { get; }

    public ImmutableDictionary<string, object?> Summary => ImmutableDictionary.CreateRange<string, object?>([
        new KeyValuePair<string, object?>("RunType", RunType),
        new KeyValuePair<string, object?>("RunId", CalculatorRunId)
    ]);
}
