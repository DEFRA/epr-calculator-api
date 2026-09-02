namespace EPR.CommonDataService.DataApi.CommonDataApi.Alignment;

public sealed record AlignmentOrganisation
{
    public required int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public Guid? SubmitterId { get; init; }
    public required string OrganisationName { get; init; }
    public string? TradingName { get; init; }
    public required string ObligationStatus { get; init; }
    public int? DaysObligated { get; init; }
    public string? JoinerDate { get; init; }
    public string? LeaverDate { get; init; }
    public string? StatusCode { get; init; }
    public string? ErrorCode { get; init; }
    public bool HasH1 { get; init; }
    public bool HasH2 { get; init; }
}
