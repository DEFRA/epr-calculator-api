using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record ProducerResult
{
    [JsonPropertyName("producerName")]
    public required string ProducerName { get; init; }

    [JsonPropertyName("producerID")]
    public required string ProducerID { get; init; }

    [JsonPropertyName("subsidiaryID")]
    public required string SubsidiaryID { get; init; }

    [JsonPropertyName("level")]
    public required int Level { get; init; }

    [JsonPropertyName("totalBill")]
    public required string TotalBill { get; init; }

    [JsonPropertyName("invoice")]
    public required ProducerInvoice Invoice { get; init; }

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

    public static ProducerResult From(
        FeeDetail producer,
        IImmutableList<MaterialDetail> materials,
        bool applyModulation) => new()
    {
        ProducerName           = producer.ProducerName,
        ProducerID             = producer.ProducerId.ToString(),
        SubsidiaryID           = producer.SubsidiaryId,
        Level                  = string.IsNullOrWhiteSpace(producer.Level) ? 1 : int.Parse(producer.Level),
        TotalBill              = producer.TotalBillBreakdown.ByCountry.Total.ToString("F2", CultureInfo.InvariantCulture),
        Invoice                = ProducerInvoice.From(producer.BillingInstruction),
        DisposalFeesByMaterial = ProducerDisposalFeesWithBadDebtProvision1
            .From(producer.DisposalFeesByMaterial, materials, producer.Level ?? "1", applyModulation)
            .MaterialBreakdown,
        DisposalCosts        = FeeWithCountries.From(producer.LADisposalCostsSection1),
        CommsCostsByMaterial = Fee.From(producer.CommsCostsSection2a),
        CommsCostsUKWide     = Fee.From(producer.CommsCostsSection2b),
        CommsCostsByCountry  = Fee.From(producer.CommsCostsSection2c),
        SaOperatingCosts     = Fee.From(producer.SaOperatingCostsSection3),
        LaDataPrepCosts      = Fee.From(producer.LaDataPrepSection4),
        SaSetUpCosts         = Fee.From(producer.SaSetupCostsSection5),
    };
}
