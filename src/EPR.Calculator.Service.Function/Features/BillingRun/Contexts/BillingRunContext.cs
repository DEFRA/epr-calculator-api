using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.BillingRun.Contexts;

/// <summary>
///     A context used exclusively for billing runs.
/// </summary>
public record BillingRunContext : RunContext
{
    /// <inheritdoc />
    public override RunType RunType => RunType.Billing;

    /// <summary>
    ///     The collection of producer IDs that were accepted by the user for billing.
    /// </summary>
    public required ImmutableHashSet<int> AcceptedProducerIds { get; init; }
}
