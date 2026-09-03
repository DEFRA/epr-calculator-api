using System.Diagnostics.CodeAnalysis;

namespace EPR.CommonDataService.DataApi.CommonDataApi.Entities;

[ExcludeFromCodeCoverage]
public record PayCalOrganisation
{
    public int? OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }
    public string? SubmitterId { get; init; }
    public string? OrganisationName { get; init; }
    public string? TradingName { get; init; }
    public string? StatusCode { get; init; }
    public string? LeaverDate { get; init; }
    public string? JoinerDate { get; init; }
    public string? RegulatorStatus { get; init; }
    public string? ObligationStatus { get; init; }
    public short? NumDaysObligated { get; init; }
    public string? ErrorCode { get; init; }
    public int? SubmissionPeriodYear { get; init; }
    public bool HasH1 { get; init; }
    public bool HasH2 { get; init; }

    // File-selection inputs only (see IAcceptedFileSelector) - not carried past that stage.
    public string? FileName { get; init; }
    public bool IsResubmission { get; init; }
    public DateTimeOffset? CreatedDateTime { get; init; }
}
