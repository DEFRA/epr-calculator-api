using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

/// <summary>
/// One constituent organisation within a producer group (2026 schema onwards): either the producer
/// itself or one of its subsidiaries.
/// </summary>
public sealed record ProducerMemberResult : ProducerFinancials
{
    /// <summary>
    /// The reporting subsidiary's own ID, or null when this member is the parent organisation
    /// reporting for itself rather than a subsidiary.
    /// </summary>
    [JsonPropertyName("subsidiaryID")]
    public required string? SubsidiaryID { get; init; }

    [JsonPropertyName("producerName")]
    public required string ProducerName { get; init; }

    public static ProducerMemberResult From(
        FeeDetail row,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation)
    {
        var f = MapFrom(row, materials, applyModulation);

        return new ProducerMemberResult
        {
            SubsidiaryID = string.IsNullOrEmpty(row.SubsidiaryId) ? null : row.SubsidiaryId,
            ProducerName = row.ProducerName,

            TotalBill              = f.TotalBill,
            DisposalFeesByMaterial = f.DisposalFeesByMaterial,
            DisposalCosts          = f.DisposalCosts,
            CommsCostsByMaterial   = f.CommsCostsByMaterial,
            CommsCostsUKWide       = f.CommsCostsUKWide,
            CommsCostsByCountry    = f.CommsCostsByCountry,
            SaOperatingCosts       = f.SaOperatingCosts,
            LaDataPrepCosts        = f.LaDataPrepCosts,
            SaSetUpCosts           = f.SaSetUpCosts,
        };
    }
}
