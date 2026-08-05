using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRuns.Outputs;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.BillingRuns;

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

            calcRun.BillingRunStatus = BillingRunStatus.Errored;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to mark billing run as {nameof(BillingRunStatus.Errored)}");
        }
    }

    private async Task SaveSuggestedBillingFees(BillingRunContext runContext, CalcResult calcResults, CancellationToken cancellationToken)
    {
        await logger.LogDuration(async () =>
        {
            var level1FeesByProducerId = calcResults.ProducerFees.Details
                .Where(f => f.FeeDetail.Level == CommonConstants.LevelOne.ToString())
                .ToImmutableDictionary(f => f.FeeDetail.ProducerId, f => f);

            if (level1FeesByProducerId.Count == 0) return;

            var suggestedInstructions = await dbContext
                .ProducerResultFileSuggestedBillingInstruction
                .Where(p => p.CalculatorRunId == runContext.RunId)
                .Where(p => level1FeesByProducerId.Keys.Contains(p.ProducerId))
                .ToListAsync(cancellationToken);

            foreach (var suggestedInstruction in suggestedInstructions)
            {
                var fee = level1FeesByProducerId[suggestedInstruction.ProducerId];
                suggestedInstruction.CurrentYearInvoiceTotalToDate = fee.FeeDetail.BillingInstruction?.CurrentYearInvoiceTotalToDate;
                suggestedInstruction.TonnageChangeSinceLastInvoice = fee.FeeDetail.BillingInstruction?.TonnageChangeSinceLastInvoice;
                suggestedInstruction.AmountLiabilityDifferenceCalcVsPrev = fee.FeeDetail.BillingInstruction?.LiabilityDifference;
                suggestedInstruction.MaterialPoundThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(fee.FeeDetail.BillingInstruction?.MaterialityLiabilityDirection);
                suggestedInstruction.TonnagePoundThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(fee.FeeDetail.BillingInstruction?.TonnageAmountLiabilityDirection);
                suggestedInstruction.PercentageLiabilityDifferenceCalcVsPrev = fee.FeeDetail.BillingInstruction?.PercentageLiabilityDifference;
                suggestedInstruction.TonnagePercentageThresholdBreached = LiabilityDirectionUtils.ToThresholdBreachedString(fee.FeeDetail.BillingInstruction?.TonnageAmountPercentageLiabilityDirection);
                suggestedInstruction.SuggestedBillingInstruction = fee.FeeDetail.BillingInstruction?.SuggestedBillingInstruction!;
                suggestedInstruction.SuggestedInvoiceAmount = fee.FeeDetail.BillingInstruction?.SuggestedInvoiceAmount ?? 0m;
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

        calcRun.BillingRunStatus = BillingRunStatus.Completed;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
