using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Api.DataApi.CommonDataApi.Entities;

[ExcludeFromCodeCoverage]
internal record PayCalPom
{
    public int OrganisationId { get; init; }
    public string? SubsidiaryId { get; init; }

    // No DB constraint backs this, but it was empirically always populated for accepted-status
    // rows when checked 2026-09-23 - kept nullable since nothing guarantees a future row won't break that.
    public string? SubmitterId { get; init; }
    public required string SubmissionPeriod { get; init; }
    public string? SubmissionPeriodDescription { get; init; }
    public string? PackagingActivity { get; init; }
    public string? PackagingType { get; init; }
    public string? PackagingClass { get; init; }
    public string? PackagingMaterial { get; init; }
    public string? PackagingMaterialSubtype { get; init; }
    public double? PackagingMaterialWeight { get; init; }
    public string? RamRagRating { get; init; }

    // File-selection inputs only (see IAcceptedFileSelector) - not read past that stage.
    public string? FileName { get; init; }
    public bool IsResubmission { get; init; }

    // Empirically always populated (checked 2026-09-23); kept nullable since no DB constraint backs it.
    [JsonPropertyName("CreatedAt")]
    public DateTime? CreatedDateTime { get; init; }
}
