using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Messaging;

/// <summary>
///     Calculator Run Parameter
/// </summary>
public record CreateBillingFileMessage : MessageBase
{
    /// <summary>
    ///     Gets or sets the user who approved the billing file.
    /// </summary>
    public required string ApprovedBy { get; init; }

    public override RunType RunType => RunType.Billing;
}
