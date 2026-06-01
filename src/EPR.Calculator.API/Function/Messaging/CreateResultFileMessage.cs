using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Messaging;

/// <summary>
///     Calculator Run Parameter
/// </summary>
public record CreateResultFileMessage : MessageBase
{
    /// <summary>
    ///     Gets or sets the relative year for the calculator run.
    /// </summary>
    public required RelativeYear RelativeYear { get; init; }

    /// <summary>
    ///     Gets or sets the user who initiated the calculator run.
    /// </summary>
    public required string CreatedBy { get; init; }

    public override RunType RunType => RunType.Calculator;
}
