using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRun.Outputs;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.BillingRun;

/// <summary>
///     Finalizes the billing run by saving any state changes to the database.
/// </summary>
public interface IBillingRunFinalizer
{
    Task FinalizeAsCompleted(BillingRunContext runContext, CalcResult calcResult, BillingFileResult exportResult, CancellationToken cancellationToken);
    Task FinalizeAsErrored(BillingRunContext runContext, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage(Justification = "Not unit testable with in-memory database given its transactional nature")]
public class BillingRunFinalizer(
    ApplicationDBContext dbContext,
    ILogger<BillingRunFinalizer> logger
)
    : IBillingRunFinalizer
{
    public async Task FinalizeAsCompleted(BillingRunContext runContext, CalcResult calcResult, BillingFileResult exportResult, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SaveSuggestedBillingFees(runContext, calcResult, cancellationToken);
            await SaveExportMetadata(exportResult, cancellationToken);
            await SaveCompletedRunStatus(runContext, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Rolling back transaction");
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RunFinalizeException(runContext.RunType, runContext.RunId, ex);
        }
    }

    public async Task FinalizeAsErrored(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var calcRun = await dbContext
                .CalculatorRuns
                .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

            calcRun.IsBillingFileGenerating = null;
            calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.ERRORID;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark billing run as failed");
        }
    }

    private async Task SaveSuggestedBillingFees(BillingRunContext runContext, CalcResult calcResults, CancellationToken cancellationToken)
    {
        await logger.LogDuration(async () =>
        {
            var level1FeesByProducerId = calcResults.CalcResultSummary.ProducerDisposalFees
                .Where(f => f.Level == CommonConstants.LevelOne.ToString())
                .ToImmutableDictionary(f => f.ProducerIdInt, f => f);

            if (level1FeesByProducerId.Count == 0) return;

            var suggestedInstructions = await dbContext
                .ProducerResultFileSuggestedBillingInstruction
                .Where(p => p.CalculatorRunId == runContext.RunId)
                .Where(p => level1FeesByProducerId.Keys.Contains(p.ProducerId))
                .ToListAsync(cancellationToken);

            foreach (var suggestedInstruction in suggestedInstructions)
            {
                var fee = level1FeesByProducerId[suggestedInstruction.ProducerId];
                suggestedInstruction.CurrentYearInvoiceTotalToDate = fee.BillingInstructionSection?.CurrentYearInvoiceTotalToDate;
                suggestedInstruction.TonnageChangeSinceLastInvoice = fee.BillingInstructionSection?.TonnageChangeSinceLastInvoice;
                suggestedInstruction.AmountLiabilityDifferenceCalcVsPrev = fee.BillingInstructionSection?.LiabilityDifference;
                suggestedInstruction.MaterialPoundThresholdBreached = fee.BillingInstructionSection?.MaterialThresholdBreached;
                suggestedInstruction.TonnagePoundThresholdBreached = fee.BillingInstructionSection?.TonnageThresholdBreached;
                suggestedInstruction.PercentageLiabilityDifferenceCalcVsPrev = fee.BillingInstructionSection?.PercentageLiabilityDifference;
                suggestedInstruction.TonnagePercentageThresholdBreached = fee.BillingInstructionSection?.TonnagePercentageThresholdBreached;
                suggestedInstruction.SuggestedBillingInstruction = fee.BillingInstructionSection?.SuggestedBillingInstruction!;
                suggestedInstruction.SuggestedInvoiceAmount = fee.BillingInstructionSection?.SuggestedInvoiceAmount ?? 0m;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        });
    }

    private async Task SaveExportMetadata(BillingFileResult exportResult, CancellationToken cancellationToken)
    {
        dbContext.CalculatorRunCsvFileMetadata.Add(exportResult.CsvMetadata);
        dbContext.CalculatorRunBillingFileMetadata.Add(exportResult.JsonMetadata);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveCompletedRunStatus(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.IsBillingFileGenerating = false;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
