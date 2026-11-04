using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.ObligationDetermination;

namespace EPR.Calculator.Api.DataApi.PomEligibility;

/// <summary>
///     A <see cref="DeterminedOrganisation" /> plus its HasH1/HasH2 flags - only ever produced by
///     <see cref="IOrganisationPeriodFlagsCalculator.ApplyPeriodFlags" />, which runs unconditionally for
///     every row. The last stage in the organisation pipeline: every downstream consumer (error
///     detection, alignment) reads this type rather than <see cref="DeterminedOrganisation" /> directly.
/// </summary>
internal sealed record FlaggedOrganisation
{
    public required PayCalOrganisation Org { get; init; }
    public required string ObligationStatus { get; init; }
    public short? NumDaysObligated { get; init; }
    public string? ErrorCode { get; init; }
    public required bool HasH1 { get; init; }
    public required bool HasH2 { get; init; }
}
