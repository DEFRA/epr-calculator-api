using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public class CalcSummaryProducerCalculationResults
{

    [JsonPropertyName("producerID")]
    public required string ProducerID { get; set; }

    [JsonPropertyName("subsidiaryID")]
    public required string SubsidiaryID { get; set; }

    [JsonPropertyName("producerName")]
    public required string ProducerName { get; set; }

    [JsonPropertyName("tradingName")]
    public string? TradingName { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("scaledUpTonnages")]
    public required string ScaledUpTonnages { get; set; }

    [JsonPropertyName("producerDisposalFeesWithBadDebtProvision1")]
    public required ProducerDisposalFeesWithBadDebtProvision1 ProducerDisposalFeesWithBadDebtProvision1 { get; set; }

    [JsonPropertyName("disposalFeeSummary1")]
    public required DisposalFeeSummary1 DisposalFeeSummary1 { get; set; }

    [JsonPropertyName("feesForCommsCostsWithBadDebtProvision2a")]
    public required CalcResultCommsCostByMaterial2AJson FeesForCommsCostsWithBadDebtProvision2a { get; set; }

    [JsonPropertyName("commsCostsByMaterialFeesSummary2a")]
    public required CalcResultSummaryCommsCostsByMaterialFeesSummary2A? CommsCostsByMaterialFeesSummary2a { get; set; }

    [JsonPropertyName("feeForLADisposalCosts1")]
    public required CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1 FeeForLADisposalCosts1 { get; set; }

    [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2a")]
    public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A FeeForCommsCostsWithBadDebtProvision_2a { get; set; }

    [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2b")]
    public required CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B FeeForCommsCostsWithBadDebtProvision_2b { get; set; }

    [JsonPropertyName("feeForCommsCostsWithBadDebtProvision_2c")]
    public required CalcResultsCommsCostsWithBadDebtProvision2C FeeForCommsCostsWithBadDebtProvision_2c { get; set; }

    [JsonPropertyName("totalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2c")]
    public required TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c { get; set; }

    [JsonPropertyName("feeForSAOperatingCostsWithBadDebtProvision_3")]
    public required CalcResultSAOperatingCostsWithBadDebtProvision FeeForSAOperatingCostsWithBadDebtProvision_3 { get; set; }

    [JsonPropertyName("feeForLADataPrepCostsWithBadDebtProvision_4")]
    public required FeeForLADataPrepCostsWithBadDebtProvision_4 FeeForLADataPrepCostsWithBadDebtProvision_4 { get; set; }

    [JsonPropertyName("feeForSASetUpCostsWithBadDebtProvision_5")]
    public required FeeForSaSetUpCostsWithBadDebtProvision5 FeeForSASetUpCostsWithBadDebtProvision_5 { get; set; }

    [JsonPropertyName("totalProducerBillWithBadDebtProvision")]
    public required TotalProducerBillWithBadDebtProvision TotalProducerBillWithBadDebtProvision { get; set; }

    [JsonPropertyName("calculationOfSuggestedBillingInstructionsAndInvoiceAmounts")]
    public required CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts { get; set; }

    public static CalcSummaryProducerCalculationResults From(ProducerFeeDetail producer, IImmutableList<MaterialDetail> materials, bool applyModulation, IReadOnlyList<int> scaledupProducerIds)
    {
        return new CalcSummaryProducerCalculationResults
            {
                ProducerID                                   = producer.FeeDetail.ProducerId.ToString(),
                SubsidiaryID                                 = producer.FeeDetail.SubsidiaryId,
                ProducerName                                 = producer.FeeDetail.ProducerName,
                TradingName                                  = producer.FeeDetail.TradingName,
                Level                                        = string.IsNullOrWhiteSpace(producer.FeeDetail.Level) ? null : int.Parse(producer.FeeDetail.Level),
                ScaledUpTonnages                             = scaledupProducerIds.Contains(producer.FeeDetail.ProducerId) ? CommonConstants.Yes : CommonConstants.No,
                ProducerDisposalFeesWithBadDebtProvision1    = ProducerDisposalFeesWithBadDebtProvision1.From(producer.FeeDetail.DisposalFeesByMaterial, materials, producer.FeeDetail.Level!, applyModulation),
                FeesForCommsCostsWithBadDebtProvision2a      = CalcResultCommsCostByMaterial2AJson.From(producer.FeeDetail.CommsFeesByMaterial, materials),
                FeeForSAOperatingCostsWithBadDebtProvision_3 = CalcResultSAOperatingCostsWithBadDebtProvision.From(producer.FeeDetail),
                FeeForLADataPrepCostsWithBadDebtProvision_4  = FeeForLADataPrepCostsWithBadDebtProvision_4.From(producer.FeeDetail),
                FeeForCommsCostsWithBadDebtProvision_2a      = CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2A.From(producer.FeeDetail),
                FeeForCommsCostsWithBadDebtProvision_2b      = CalcResultSummaryFeeForCommsCostsWithBadDebtProvision2B.From(producer.FeeDetail),
                CommsCostsByMaterialFeesSummary2a            = CalcResultSummaryCommsCostsByMaterialFeesSummary2A.From(producer.FeeDetail),
                TotalProducerFeeWithBadDebtProvisionFor2con_1_2a_2b_2c = TotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper.From(producer.FeeDetail),
                FeeForSASetUpCostsWithBadDebtProvision_5     = FeeForSaSetUpCostsWithBadDebtProvision5.From(producer.FeeDetail),
                FeeForCommsCostsWithBadDebtProvision_2c      = CalcResultsCommsCostsWithBadDebtProvision2C.From(producer.FeeDetail),
                CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts = CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts.From(producer.FeeDetail),
                TotalProducerBillWithBadDebtProvision        = TotalProducerBillWithBadDebtProvision.From(producer.FeeDetail),
                FeeForLADisposalCosts1                       = CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1.From(producer.FeeDetail),
                DisposalFeeSummary1                          = DisposalFeeSummary1.From(producer.FeeDetail),
            };
    }
}
