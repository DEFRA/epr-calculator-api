using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

/// <summary>
/// The financial breakdown for one organisation's worth of billing data (2026 schema onwards).
/// Shared by a producer group's own aggregate totals (<see cref="ProducerGroupResult"/>) and by
/// each member of that group (<see cref="ProducerMemberResult"/>).
/// </summary>
public abstract record ProducerFinancials
{
    [JsonPropertyName("totalBill")]
    public required string TotalBill { get; init; }

    [JsonPropertyName("disposalFeesByMaterial")]
    public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> DisposalFeesByMaterial { get; init; }

    [JsonPropertyName("disposalCosts")]
    public required FeeWithCountries DisposalCosts { get; init; }

    [JsonPropertyName("commsCostsByMaterial")]
    public required Fee CommsCostsByMaterial { get; init; }

    [JsonPropertyName("commsCostsUKWide")]
    public required Fee CommsCostsUKWide { get; init; }

    [JsonPropertyName("commsCostsByCountry")]
    public required Fee CommsCostsByCountry { get; init; }

    [JsonPropertyName("saOperatingCosts")]
    public required Fee SaOperatingCosts { get; init; }

    [JsonPropertyName("laDataPrepCosts")]
    public required Fee LaDataPrepCosts { get; init; }

    [JsonPropertyName("saSetUpCosts")]
    public required Fee SaSetUpCosts { get; init; }

    protected static (
        string TotalBill,
        IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> DisposalFeesByMaterial,
        FeeWithCountries DisposalCosts,
        Fee CommsCostsByMaterial,
        Fee CommsCostsUKWide,
        Fee CommsCostsByCountry,
        Fee SaOperatingCosts,
        Fee LaDataPrepCosts,
        Fee SaSetUpCosts
    ) MapFrom(
        FeeDetail row,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation) => (
        TotalBill: row.TotalBillBreakdown.ByCountry.Total.ToString("F2", CultureInfo.InvariantCulture),
        DisposalFeesByMaterial: ProducerDisposalFeesWithBadDebtProvision1
            .From(row.DisposalFeesByMaterial, materials, row.Level ?? "1", applyModulation)
            .MaterialBreakdown,
        DisposalCosts: FeeWithCountries.From(row.LADisposalCostsSection1),
        CommsCostsByMaterial: Fee.From(row.CommsCostsSection2a),
        CommsCostsUKWide: Fee.From(row.CommsCostsSection2b),
        CommsCostsByCountry: Fee.From(row.CommsCostsSection2c),
        SaOperatingCosts: Fee.From(row.SaOperatingCostsSection3),
        LaDataPrepCosts: Fee.From(row.LaDataPrepSection4),
        SaSetUpCosts: Fee.From(row.SaSetupCostsSection5)
    );
}
