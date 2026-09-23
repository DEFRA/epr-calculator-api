using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;

namespace EPR.Calculator.Api.DataApi.ObligationDetermination;

/// <summary>
///     A <see cref="PayCalOrganisation" /> plus its obligation determination result - only ever produced
///     by <see cref="IProducerObligationDeterminer.Determine" />, which runs unconditionally for every
///     row, so <see cref="ObligationStatus" /> is always set. <see cref="NumDaysObligated" /> and
///     <see cref="ErrorCode" /> stay optional even here - most rows simply don't have one.
/// </summary>
internal sealed record DeterminedOrganisation
{
    public required PayCalOrganisation Org { get; init; }
    public required string ObligationStatus { get; init; }
    public short? NumDaysObligated { get; init; }
    public string? ErrorCode { get; init; }
}
