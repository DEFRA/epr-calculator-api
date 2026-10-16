namespace EPR.CommonDataService.DataApi.Alignment;

/// <summary>
///     A material reported by a producer for a submission period, aggregated across RAG ratings.
///     Weights are un-rounded totals in the source unit (kg) - converting to tonnage and rounding
///     for storage is the caller's responsibility.
/// </summary>
public sealed record AlignedReportedMaterial
{
    public required string MaterialCode { get; init; }
    public required string PackagingType { get; init; }
    public required string SubmissionPeriod { get; init; }
    public double TotalWeight { get; init; }
    public double RedWeight { get; init; }
    public double AmberWeight { get; init; }
    public double GreenWeight { get; init; }
    public double RedMedicalWeight { get; init; }
    public double AmberMedicalWeight { get; init; }
    public double GreenMedicalWeight { get; init; }
}
