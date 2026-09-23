using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Api.DataApi.CommonDataApi.Entities;

[ExcludeFromCodeCoverage]
internal record PayCalOrganisation
{
    public int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }

    // No DB constraint backs this, but it was empirically always populated for accepted-status
    // rows when checked 2026-09-23 - kept nullable since nothing guarantees a future row won't break that.
    public string? SubmitterId { get; init; }
    public required string OrganisationName { get; init; }
    public string? TradingName { get; init; }

    [JsonPropertyName("LeaverCode")]
    public string? StatusCode { get; init; }
    public string? LeaverDate { get; init; }
    public string? JoinerDate { get; init; }
    public required string RegulatorStatus { get; init; }
    public required int SubmissionPeriodYear { get; init; }

    // File-selection inputs only (see IAcceptedFileSelector) - not read past that stage.
    public string? FileName { get; init; }
    public bool IsResubmission { get; init; }

    // Empirically always populated (checked 2026-09-23); kept nullable since no DB constraint backs it.
    [JsonPropertyName("CreatedAt")]
    public DateTime? CreatedDateTime { get; init; }
}
