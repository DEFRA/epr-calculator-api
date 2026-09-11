using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Utils;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns;

/// <summary>
///     Finalizes the billing run by saving any state changes to the database.
/// </summary>
public interface IBillingRunFinalizer
{
    /// <param name="level1BillingInstructions">
    ///     The Level-1 <see cref="BillingInstruction" /> per producer, keyed by producer ID - collected as
    ///     a side effect of the CSV/JSON export's own pass over the fee-detail stream (see
    ///     <see cref="BillingRuns.BillingRunProcessor" />), so finalizing doesn't need a third full pass
    ///     over the ~10^4-row, owned-JSON fee graph just to read this one small sub-object back out.
    /// </param>
    Task FinalizeAsCompleted(BillingRunContext runContext, CalcResult calcResult, BillingFileResult exportResult, IReadOnlyDictionary<int, BillingInstruction?> level1BillingInstructions, CancellationToken cancellationToken);
    Task FinalizeAsErrored(BillingRunContext runContext, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage(Justification = "Not unit testable with in-memory database given its transactional nature")]
public class BillingRunFinalizer(
    ApplicationDBContext dbContext,
    ILogger<BillingRunFinalizer> logger
) : IBillingRunFinalizer
{
    [ActivityTrace]
    public async Task FinalizeAsCompleted(BillingRunContext runContext, CalcResult calcResult, BillingFileResult exportResult, IReadOnlyDictionary<int, BillingInstruction?> level1BillingInstructions, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await SaveSuggestedBillingFees(runContext, level1BillingInstructions, cancellationToken);
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

            calcRun.BillingRunStatus = BillingRunStatus.Errored;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to mark billing run as {nameof(BillingRunStatus.Errored)}");
        }
    }

    [ActivityTrace]
    private async Task SaveSuggestedBillingFees(BillingRunContext runContext, IReadOnlyDictionary<int, BillingInstruction?> level1BillingInstructions, CancellationToken cancellationToken)
    {
        if (level1BillingInstructions.Count == 0) return;

        var suggestedInstructions = await dbContext
            .ProducerResultFileSuggestedBillingInstruction
            .Where(p => p.CalculatorRunId == runContext.RunId)
            .Where(p => level1BillingInstructions.Keys.Contains(p.ProducerId))
            .ToListAsync(cancellationToken);

        foreach (var suggestedInstruction in suggestedInstructions)
        {
            var billingInstruction = level1BillingInstructions[suggestedInstruction.ProducerId];
            suggestedInstruction.CurrentYearInvoiceTotalToDate = billingInstruction?.CurrentYearInvoiceTotalToDate;
            suggestedInstruction.TonnageChangeSinceLastInvoice = billingInstruction?.TonnageChangeSinceLastInvoice;
            suggestedInstruction.AmountLiabilityDifferenceCalcVsPrev = billingInstruction?.LiabilityDifference;
            suggestedInstruction.MaterialPoundThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(billingInstruction?.MaterialityLiabilityDirection);
            suggestedInstruction.TonnagePoundThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(billingInstruction?.TonnageAmountLiabilityDirection);
            suggestedInstruction.PercentageLiabilityDifferenceCalcVsPrev = billingInstruction?.PercentageLiabilityDifference;
            suggestedInstruction.TonnagePercentageThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(billingInstruction?.TonnageAmountPercentageLiabilityDirection);
            suggestedInstruction.SuggestedBillingInstruction = billingInstruction?.SuggestedBillingInstruction!;
            suggestedInstruction.SuggestedInvoiceAmount = billingInstruction?.SuggestedInvoiceAmount ?? 0m;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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

        calcRun.BillingRunStatus = BillingRunStatus.Completed;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
