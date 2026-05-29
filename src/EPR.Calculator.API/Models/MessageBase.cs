using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.API.Models;

/// <summary>
///     Represents the base class for all message types.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract record MessageBase
{
    public required int CalculatorRunId { get; init; }
    public required string RunName { get; init; } // For filenames
    public required string MessageType { get; init; }

    public ImmutableDictionary<string, object?> Summary => ImmutableDictionary.CreateRange<string, object?>([
        new KeyValuePair<string, object?>("MessageType", MessageType),
        new KeyValuePair<string, object?>("RunId", CalculatorRunId)
    ]);
}
