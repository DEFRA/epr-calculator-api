namespace EPR.CommonDataService.DataApi.CommonDataApi.Alignment;

public sealed record AlignmentOrganisation
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public Guid? SubmitterId { get; init; }
    public required string OrganisationName { get; init; }
    public string? TradingName { get; init; }
    public required string ObligationStatus { get; init; }
    public bool HasH2 { get; init; }
}
