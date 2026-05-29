using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public class BillingInstructionService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    ILogger<BillingInstructionService> logger)
    : IBillingInstructionService
{
    public Task<bool> CreateBillingInstructions(CalcResult calcResult) =>
        logger.LogDuration(async () =>
        {
            try
            {
                var billingInstructions = GetBillingInstructions(calcResult);

                if (billingInstructions.Count == 0)
                {
                    logger.LogError("No billing instructions generated");
                    return false;
                }

                await bulkOps.BulkInsertAsync(dbContext, billingInstructions);

                logger.LogInformation("Inserted {BillingInstructionsCount} billing instructions", billingInstructions.Count);
                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while populating the billing instructions");
                return false;
            }
        });

    private static ImmutableList<ProducerResultFileSuggestedBillingInstruction> GetBillingInstructions(CalcResult calcResult)
    {
        var producers = calcResult.CalcResultSummary.ProducerDisposalFees
            .Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

        var cancelledProducers = calcResult.CalcResultCancelledProducers;

        var billingInstructions = ImmutableList.CreateBuilder<ProducerResultFileSuggestedBillingInstruction>();

        foreach (var producer in producers)
        {
            if (!int.TryParse(producer.ProducerId, out var producerId))
                continue;

            var billingInstructionSection = producer.BillingInstructionSection;

            var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = calcResult.CalcResultDetail.RunId,
                ProducerId = producerId,
                TotalProducerBillWithBadDebt = producer.TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision,
                CurrentYearInvoiceTotalToDate = billingInstructionSection?.CurrentYearInvoiceTotalToDate,
                TonnageChangeSinceLastInvoice = GetStringValue(billingInstructionSection?.TonnageChangeSinceLastInvoice!),
                AmountLiabilityDifferenceCalcVsPrev = billingInstructionSection?.LiabilityDifference!,
                MaterialPoundThresholdBreached = GetStringValue(billingInstructionSection?.MaterialThresholdBreached!),
                TonnagePoundThresholdBreached = GetStringValue(billingInstructionSection?.TonnageThresholdBreached!),
                PercentageLiabilityDifferenceCalcVsPrev = billingInstructionSection?.PercentageLiabilityDifference!,
                MaterialPercentageThresholdBreached = GetStringValue(billingInstructionSection?.MaterialPercentageThresholdBreached!),
                TonnagePercentageThresholdBreached = GetStringValue(billingInstructionSection?.TonnagePercentageThresholdBreached!),
                SuggestedBillingInstruction = billingInstructionSection?.SuggestedBillingInstruction!,
                SuggestedInvoiceAmount = billingInstructionSection?.SuggestedInvoiceAmount ?? 0m
            };

            billingInstructions.Add(billingInstruction);
        }

        foreach (var cancelledProducer in cancelledProducers.CancelledProducers)
        {
            var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = calcResult.CalcResultDetail.RunId,
                ProducerId = cancelledProducer.ProducerId,
                TotalProducerBillWithBadDebt = null,
                CurrentYearInvoiceTotalToDate = cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue,
                TonnageChangeSinceLastInvoice = null,
                AmountLiabilityDifferenceCalcVsPrev = null,
                MaterialPoundThresholdBreached = null,
                TonnagePoundThresholdBreached = null,
                PercentageLiabilityDifferenceCalcVsPrev = null,
                MaterialPercentageThresholdBreached = null,
                TonnagePercentageThresholdBreached = null,
                SuggestedBillingInstruction = CommonConstants.CancelStatus,
                SuggestedInvoiceAmount = null
            };
            billingInstructions.Add(billingInstruction);
        }

        return billingInstructions.ToImmutable();
    }

    private static bool IsDefaultValue(string? value) =>
        string.IsNullOrEmpty(value) || value == CommonConstants.Hyphen;

    private static string? GetStringValue(string value)
    {
        return IsDefaultValue(value)
            ? null
            : TypeConverterUtil.ConvertTo<string>(value);
    }
}
