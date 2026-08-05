using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

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

    public static CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts From(FeeDetail fees)
    {
        string GetPercentageLiabilityDifference(decimal? percentageLiabilityDifference) =>
            percentageLiabilityDifference == null
                ? CommonConstants.Hyphen
                : FormatUtils.FormatPercentage(percentageLiabilityDifference.Value, 2);

        string GetFormattedCurrencyValue(decimal? value) =>
            value == null
                ? CommonConstants.Hyphen
                : FormatUtils.FormatCurrency(value.Value);

        var costs = fees.BillingInstruction;

        return new CalculationOfSuggestedBillingInstructionsAndInvoiceAmounts
        {
            CurrentYearInvoicedTotalToDate          = GetFormattedCurrencyValue(costs!.CurrentYearInvoiceTotalToDate),
            TonnageChangeSinceLastInvoice           = costs.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen,
            LiabilityDifferenceCalcVsPrev           = GetFormattedCurrencyValue(costs.LiabilityDifference),
            MaterialThresholdBreached               = LiabilityDirectionUtils.ToThresholdBreachedString(costs.MaterialityLiabilityDirection),
            TonnageThresholdBreached                = LiabilityDirectionUtils.ToThresholdBreachedString(costs.TonnageAmountLiabilityDirection),
            PercentageLiabilityDifferenceCalcVsPrev = GetPercentageLiabilityDifference(costs.PercentageLiabilityDifference),
            MaterialPercentageThresholdBreached     = LiabilityDirectionUtils.ToThresholdBreachedString(costs.MaterialityPercentageLiabilityDirection),
            TonnagePercentageThresholdBreached      = LiabilityDirectionUtils.ToThresholdBreachedString(costs.TonnageAmountPercentageLiabilityDirection),
            SuggestedBillingInstruction             = costs.SuggestedBillingInstruction ?? CommonConstants.Hyphen,
            SuggestedInvoiceAmount                  = GetFormattedCurrencyValue(costs.SuggestedInvoiceAmount!)
        };
    }
}
