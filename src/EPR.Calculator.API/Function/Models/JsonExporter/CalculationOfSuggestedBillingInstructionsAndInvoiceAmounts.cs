using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
    {
        [JsonPropertyName("currentYearInvoicedTotalToDate")]
        public required string? CurrentYearInvoicedTotalToDate { get; init; }

        [JsonPropertyName("tonnageChangeSinceLastInvoice")]
        public required string TonnageChangeSinceLastInvoice { get; init; }

        [JsonPropertyName("liabilityDifferenceCalcVsPrev")]
        public required string? LiabilityDifferenceCalcVsPrev { get; init; }

        [JsonPropertyName("material£ThresholdBreached")]
        public required string MaterialThresholdBreached { get; init; }

        [JsonPropertyName("tonnage£ThresholdBreached")]
        public required string TonnageThresholdBreached { get; init; }

        [JsonPropertyName("percentageLiabilityDifferenceCalcVsPrev")]
        public required string PercentageLiabilityDifferenceCalcVsPrev { get; init; }

        [JsonPropertyName("materialPercentageThresholdBreached")]
        public required string MaterialPercentageThresholdBreached { get; init; }

        [JsonPropertyName("tonnagePercentageThresholdBreached")]
        public required string TonnagePercentageThresholdBreached { get; init; }

        [JsonPropertyName("suggestedBillingInstruction")]
        public required string SuggestedBillingInstruction { get; init; }

        [JsonPropertyName("suggestedInvoiceAmount")]
        public required string SuggestedInvoiceAmount { get; init; }

        public static CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts From(CalcResultSummaryProducerDisposalFees fees)
        {
            string GetPercentageLiabilityDifference(decimal? percentageLiabilityDifference)
            {
                if (percentageLiabilityDifference == null)
                    return CommonConstants.Hyphen;

                return $"{Math.Round((decimal)percentageLiabilityDifference, (int)DecimalPlaces.Two).ToString()}%";
            }

            string GetFormattedCurrencyValue(decimal? value)
            {
                if (value == null)
                    return CommonConstants.Hyphen;

                return CurrencyConverterUtils.ConvertToCurrency(value.Value);
            }

            var costs = fees.BillingInstructionSection;
            return new CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
            {
                CurrentYearInvoicedTotalToDate = GetFormattedCurrencyValue(costs!.CurrentYearInvoiceTotalToDate),
                TonnageChangeSinceLastInvoice = costs.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen,
                LiabilityDifferenceCalcVsPrev = GetFormattedCurrencyValue(costs.LiabilityDifference),
                MaterialThresholdBreached = costs.MaterialThresholdBreached ?? CommonConstants.Hyphen,
                TonnageThresholdBreached = costs.TonnageThresholdBreached ?? CommonConstants.Hyphen,
                PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(costs.PercentageLiabilityDifference),
                MaterialPercentageThresholdBreached = costs.MaterialPercentageThresholdBreached ?? CommonConstants.Hyphen,
                TonnagePercentageThresholdBreached = costs.TonnagePercentageThresholdBreached ?? CommonConstants.Hyphen,
                SuggestedBillingInstruction = costs.SuggestedBillingInstruction ?? CommonConstants.Hyphen,
                SuggestedInvoiceAmount = GetFormattedCurrencyValue(costs.SuggestedInvoiceAmount!)
            };
        }
    }
}
