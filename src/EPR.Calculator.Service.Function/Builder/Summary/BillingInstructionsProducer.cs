using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Builder.Summary;

public static class BillingInstructionsProducer
{
    public static void SetValues(ProducerFees result, IReadOnlyList<InvoicedProducer> ProducerInvoicedMaterialNetTonnage, CalcResultParameterOtherCost otherCost)
    {
        decimal totalTonnage = 0;
        decimal liabilityDifferenceRunningTotal = 0m;
        decimal SuggestedInvoiceAmountTotal = 0m;

        var param_MATT_AI = otherCost.MaterialityIncrease.Amount;
        var param_MATT_AD = otherCost.MaterialityDecrease.Amount;
        var param_TONT_AI = otherCost.TonnageChangeIncrease.Amount;
        var param_TONT_AD = otherCost.TonnageChangeDecrease.Amount;
        var param_MATT_PI = otherCost.MaterialityIncrease.Percentage;
        var param_MATT_PD = otherCost.MaterialityDecrease.Percentage;
        var param_TONT_PI = otherCost.TonnageChangeIncrease.Percentage;
        var param_TONT_PD = otherCost.TonnageChangeDecrease.Percentage;

        // PERF: Pre-index the invoiced records by ProducerId (as string) once.
        // Replaces O(fees * invoiced records) scan that previously ran inside the loop.
        var currentYearInvoicedByProducerId = new Dictionary<int, decimal?>();
        foreach (var record in ProducerInvoicedMaterialNetTonnage)
        {
            // Preserves the original semantics of choosing the first encountered record per producerId.
            currentYearInvoicedByProducerId.TryAdd(record.ProducerId, record.CurrentYearInvoicedTotalAfterThisRun);
        }

        foreach (var fee in result.Details)
        {
            currentYearInvoicedByProducerId.TryGetValue(fee.FeeDetail.ProducerId, out var currentYearInvoicedTotalTonnage);

            totalTonnage += currentYearInvoicedTotalTonnage.GetValueOrDefault();

            var liabilityDifferenceCalculated        = CalculateLiabilityDifference(fee, currentYearInvoicedTotalTonnage);
            if (liabilityDifferenceCalculated.HasValue)
                liabilityDifferenceRunningTotal     += liabilityDifferenceCalculated.Value;
            var currentYearInvoiceTotalToDate        = GetCurrentYearInvoicedTotalToDate(fee, currentYearInvoicedTotalTonnage);
            var tonnageChangeSinceLastInvoice        = GetTonnageChangeSinceLastInvoice(fee);
            var liabilityDifference                  = liabilityDifferenceCalculated;
            var percentageLiabilityDifference        = GetPercentageLiabilityDifference(fee, currentYearInvoiceTotalToDate, liabilityDifference);
            var materialLiabilityDirection           = GetMaterialLiabilityDirection(fee, currentYearInvoicedTotalTonnage, liabilityDifferenceCalculated, param_MATT_AI, param_MATT_AD);
            var tonnageLiabilityDirection            = GetTonnageLiabilityDirection(fee, currentYearInvoicedTotalTonnage, liabilityDifferenceCalculated, param_TONT_AI, param_TONT_AD);
            var materialPercentageLiabilityDirection = GetMaterialPercentageLiabilityDirection(fee, currentYearInvoiceTotalToDate, percentageLiabilityDifference, param_MATT_PI, param_MATT_PD);
            var tonnagePercentageLiabilityDirection  = GetTonnagePercentageLiabilityDirection(fee, currentYearInvoiceTotalToDate, tonnageChangeSinceLastInvoice, percentageLiabilityDifference, param_TONT_PI, param_TONT_PD);
            var suggestedBillingInstruction          = GetSuggestedBillingInstruction(fee, currentYearInvoiceTotalToDate, liabilityDifference, materialLiabilityDirection, tonnageLiabilityDirection, materialPercentageLiabilityDirection, tonnagePercentageLiabilityDirection);
            var suggestedInvoiceAmount               = GetSuggestedInvoiceAmount(fee, suggestedBillingInstruction, liabilityDifference);
            if (suggestedInvoiceAmount.HasValue)
                SuggestedInvoiceAmountTotal         += suggestedInvoiceAmount.Value;

            fee.FeeDetail.BillingInstruction = new BillingInstruction
            {
                CurrentYearInvoiceTotalToDate              = currentYearInvoiceTotalToDate,
                TonnageChangeSinceLastInvoice              = tonnageChangeSinceLastInvoice,
                LiabilityDifference                        = liabilityDifference,
                MaterialityLiabilityDirection              = materialLiabilityDirection,
                TonnageAmountLiabilityDirection            = tonnageLiabilityDirection,
                PercentageLiabilityDifference              = percentageLiabilityDifference,
                MaterialityPercentageLiabilityDirection    = materialPercentageLiabilityDirection,
                TonnageAmountPercentageLiabilityDirection  = tonnagePercentageLiabilityDirection,
                SuggestedBillingInstruction                = suggestedBillingInstruction,
                SuggestedInvoiceAmount                     = suggestedInvoiceAmount
            };
        }

        result.Total.BillingInstruction = new BillingInstruction
        {
            CurrentYearInvoiceTotalToDate               = totalTonnage,
            TonnageChangeSinceLastInvoice               = string.Empty,
            LiabilityDifference                         = liabilityDifferenceRunningTotal == 0m ? null : liabilityDifferenceRunningTotal,
            MaterialityLiabilityDirection               = null,
            TonnageAmountLiabilityDirection             = null,
            PercentageLiabilityDifference               = null,
            MaterialityPercentageLiabilityDirection     = null,
            TonnageAmountPercentageLiabilityDirection   = null,
            SuggestedBillingInstruction                 = string.Empty,
            SuggestedInvoiceAmount                      = SuggestedInvoiceAmountTotal
        };
    }

    private static decimal? GetCurrentYearInvoicedTotalToDate(ProducerFeeDetail fee, decimal? currentYearInvoicedTotalTonnage)
    {
        if (fee.FeeDetail.Level == "1")
            return currentYearInvoicedTotalTonnage;

        return null;
    }

    private static string? GetTonnageChangeSinceLastInvoice(
        ProducerFeeDetail fee
    ) => fee.FeeDetail.TonnageChangeAdvice == "CHANGE" ? "Tonnage Changed" : null;

    private static decimal? CalculateLiabilityDifference(
        ProducerFeeDetail fee,
        decimal? currentInvoicedTotalToDate
    ) =>
        (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) || (!currentInvoicedTotalToDate.HasValue)
        ? null
        : MathUtils.RoundAwayFromZero(fee.FeeDetail.TotalBillBreakdown.ByCountry.Total, 2) - MathUtils.RoundAwayFromZero(currentInvoicedTotalToDate.Value, 2);

    private static LiabilityDirection? GetMaterialLiabilityDirection(ProducerFeeDetail fee, decimal? currentInvoicedTotalToDate, decimal? liabilityDifferenceCalculated, decimal param_MATT_AI, decimal param_MATT_AD)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return null;
        if (!currentInvoicedTotalToDate.HasValue) return null;
        if (!liabilityDifferenceCalculated.HasValue) return null;

        if (liabilityDifferenceCalculated >= param_MATT_AI) return LiabilityDirection.Positive;
        if (liabilityDifferenceCalculated <= param_MATT_AD) return LiabilityDirection.Negative;

        return null;
    }

    private static LiabilityDirection? GetTonnageLiabilityDirection(ProducerFeeDetail fee, decimal? currentInvoicedTotalToDate, decimal? liabilityDifferenceCalculated, decimal param_TONT_AI, decimal param_TONT_AD)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return null;
        if (!currentInvoicedTotalToDate.HasValue) return null;
        if (fee.FeeDetail.TonnageChangeAdvice != "CHANGE") return null;
        if (!liabilityDifferenceCalculated.HasValue) return null;

        if (liabilityDifferenceCalculated >= param_TONT_AI) return LiabilityDirection.Positive;
        if (liabilityDifferenceCalculated <= param_TONT_AD) return LiabilityDirection.Negative;

        return null;
    }

    private static decimal? GetPercentageLiabilityDifference(
        ProducerFeeDetail fee,
        decimal? currentYearInvoiceTotalToDate,
        decimal? liabilityDifference
    ) =>
        (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()
        || !currentYearInvoiceTotalToDate.HasValue
        || !liabilityDifference.HasValue
        || currentYearInvoiceTotalToDate == 0m
        )
        ? null
        : MathUtils.RoundAwayFromZero(liabilityDifference.Value / currentYearInvoiceTotalToDate.Value * 100, 2);

    private static LiabilityDirection? GetMaterialPercentageLiabilityDirection(ProducerFeeDetail fee, decimal? currentYearInvoiceTotalToDate, decimal? percentageLiabilityDifference, decimal param_MATT_PI, decimal param_MATT_PD)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return null;
        if (!currentYearInvoiceTotalToDate.HasValue) return null;

        if (percentageLiabilityDifference >= param_MATT_PI) return LiabilityDirection.Positive;
        if (percentageLiabilityDifference <= param_MATT_PD) return LiabilityDirection.Negative;

        return null;
    }

    private static LiabilityDirection? GetTonnagePercentageLiabilityDirection(ProducerFeeDetail fee, decimal? currentYearInvoiceTotalToDate, string? tonnageChangeSinceLastInvoice, decimal? percentageLiabilityDifference, decimal param_TONT_PI, decimal param_TONT_PD)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return null;

        if (!currentYearInvoiceTotalToDate.HasValue) return null;
        if (tonnageChangeSinceLastInvoice != CommonConstants.TonnageChanged) return null;

        if (percentageLiabilityDifference >= param_TONT_PI) return LiabilityDirection.Positive;
        if (percentageLiabilityDifference <= param_TONT_PD) return LiabilityDirection.Negative;

        return null;
    }

    private static string GetSuggestedBillingInstruction(ProducerFeeDetail fee, decimal? currentYearInvoiceTotalToDate, decimal? liabilityDifference, LiabilityDirection? materialThresholdBreached, LiabilityDirection? tonnageThresholdBreached, LiabilityDirection? materialPercentageThresholdBreached, LiabilityDirection? tonnagePercentageThresholdBreached)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return CommonConstants.Hyphen;

        if (!currentYearInvoiceTotalToDate.HasValue) return CommonConstants.Initial;

        if (liabilityDifference > 0 &&
            (materialThresholdBreached != null || tonnageThresholdBreached != null || materialPercentageThresholdBreached != null || tonnagePercentageThresholdBreached != null))
            return BillingConstants.Suggestion.Delta;

        if (liabilityDifference < 0 &&
            (materialThresholdBreached != null || tonnageThresholdBreached != null || materialPercentageThresholdBreached != null || tonnagePercentageThresholdBreached != null))
            return BillingConstants.Suggestion.Rebill;

        return CommonConstants.Hyphen;
    }

    private static decimal? GetSuggestedInvoiceAmount(ProducerFeeDetail fee, string suggestedBillingInstruction, decimal? liabilityDifference)
    {
        if (fee.FeeDetail.Level != CommonConstants.LevelOne.ToString()) return null;

        if (suggestedBillingInstruction is BillingConstants.Suggestion.Initial or BillingConstants.Suggestion.Rebill)
            return fee.FeeDetail.TotalBillBreakdown?.ByCountry.Total;

        if (suggestedBillingInstruction == BillingConstants.Suggestion.Delta) return liabilityDifference;

        return null;
    }
}
