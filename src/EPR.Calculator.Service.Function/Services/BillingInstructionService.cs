using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

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
        var producers = calcResult.ProducerFees.Details
            .Where(producer => producer.FeeDetail.Level == CommonConstants.LevelOne.ToString());

        var billingInstructions = ImmutableList.CreateBuilder<ProducerResultFileSuggestedBillingInstruction>();

        foreach (var producer in producers.Select(fee => fee.FeeDetail))
        {
            var billingInstructionSection = producer.BillingInstruction;

            var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId                         = calcResult.CalcResultDetail.RunId,
                ProducerId                              = producer.ProducerId,
                TotalProducerBillWithBadDebt            = producer.TotalBillBreakdown!.ByCountry.Total,
                CurrentYearInvoiceTotalToDate           = billingInstructionSection?.CurrentYearInvoiceTotalToDate,
                TonnageChangeSinceLastInvoice           = GetStringValue(billingInstructionSection?.TonnageChangeSinceLastInvoice!),
                AmountLiabilityDifferenceCalcVsPrev     = billingInstructionSection?.LiabilityDifference!,
                MaterialPoundThresholdBreached          = GetStringValue(billingInstructionSection?.MaterialityLiabilityDirection),
                TonnagePoundThresholdBreached           = GetStringValue(billingInstructionSection?.TonnageAmountLiabilityDirection),
                PercentageLiabilityDifferenceCalcVsPrev = billingInstructionSection?.PercentageLiabilityDifference!,
                MaterialPercentageThresholdBreached     = GetStringValue(billingInstructionSection?.MaterialityPercentageLiabilityDirection),
                TonnagePercentageThresholdBreached      = GetStringValue(billingInstructionSection?.TonnageAmountPercentageLiabilityDirection),
                SuggestedBillingInstruction             = billingInstructionSection?.SuggestedBillingInstruction!,
                SuggestedInvoiceAmount                  = billingInstructionSection?.SuggestedInvoiceAmount ?? 0m
            };

            billingInstructions.Add(billingInstruction);
        }

        foreach (var cancelledProducer in calcResult.CalcResultCancelledProducers)
        {
            var billingInstruction = new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = calcResult.CalcResultDetail.RunId,
                ProducerId = cancelledProducer.ProducerId,
                TotalProducerBillWithBadDebt = null,
                CurrentYearInvoiceTotalToDate = cancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDate,
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

    private static string? GetStringValue(LiabilityDirection? value) =>
        GetStringValue(LiabilityDirectionUtils.ToThresholdBreachedString(value));
}
