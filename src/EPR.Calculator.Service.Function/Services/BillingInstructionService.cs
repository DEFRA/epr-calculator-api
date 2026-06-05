using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRun.Constants;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public interface IBillingInstructionService
{
    Task CreateBillingInstructions(CalculatorRunContext runContext, CalcResult calcResult);
}

public class BillingInstructionService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    ILogger<BillingInstructionService> logger)
    : IBillingInstructionService
{
    public Task CreateBillingInstructions(CalculatorRunContext runContext, CalcResult calcResult) =>
        logger.LogDuration(async () =>
        {
            try
            {
                var billingInstructions = GetBillingInstructions(calcResult);

                if (billingInstructions.Count == 0)
                    throw new RunProcessingException(runContext, "No billing instructions generated");

                await bulkOps.BulkInsertAsync(dbContext, billingInstructions);

                logger.LogInformation("Inserted {BillingInstructionsCount} billing instructions", billingInstructions.Count);
            }
            catch (Exception exception)
            {
                throw new RunProcessingException(runContext, "Error occurred while generating billing instructions, see inner exception for details.", exception);
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
                SuggestedBillingInstruction = BillingConstants.Suggestion.Cancel,
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
