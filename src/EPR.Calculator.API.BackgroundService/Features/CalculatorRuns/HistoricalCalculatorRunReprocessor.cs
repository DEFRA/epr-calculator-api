using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface IHistoricalCalculatorRunReprocessor
{
    Task ReprocessAsync(CancellationToken cancellationToken);
}

public class HistoricalCalculatorRunReprocessor(
    ApplicationDBContext dbContext,
    IParameterService parameterService,
    IResultBuilder resultBuilder,
    TimeProvider timeProvider,
    ILogger<HistoricalCalculatorRunReprocessor> logger
) : IHistoricalCalculatorRunReprocessor
{
    public async Task ReprocessAsync(CancellationToken cancellationToken)
    {
        var runsToBeReprocessed =
            await dbContext.CalculatorRuns
                .Where(cr => !dbContext.ProducerDisposalFee.Any(pf => pf.CalculatorRunId == cr.Id))
                .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} historical calculator runs needing reprocessing", runsToBeReprocessed.Count);
        logger.LogInformation("RunIds: {RunIds}", string.Join(", ", runsToBeReprocessed.Select(r => r.Id)));

        var total = runsToBeReprocessed.Count;
        var failedRunIds = new List<int>();

        foreach (var (run, index) in runsToBeReprocessed.Select((run, i) => (run, i + 1)))
        {
            try
            {
                logger.LogInformation("Reprocessing calculator run with ID: {RunId} ({Index}/{Total})", run.Id, index, total);
                await ReprocessRunAsync(run, cancellationToken);
                logger.LogInformation("Successfully reprocessed calculator run with ID: {RunId} ({Index}/{Total})", run.Id, index, total);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reprocessing calculator run with ID: {RunId} ({Index}/{Total})", run.Id, index, total);
                failedRunIds.Add(run.Id);

                dbContext.ChangeTracker.Clear();
            }
        }

        logger.LogInformation(
            "Finished reprocessing historical calculator runs. Succeeded: {SucceededCount}/{Total}, Failed: {FailedCount}",
            total - failedRunIds.Count, total, failedRunIds.Count);

        if (failedRunIds.Count > 0)
            logger.LogWarning("Failed RunIds: {FailedRunIds}", string.Join(", ", failedRunIds));
    }

    private async Task ReprocessRunAsync(CalculatorRun run, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await DeleteExistingRunResultsAsync(run.Id, cancellationToken);

        var runContext = new CalculatorRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = timeProvider.GetUtcNow(),
            RelativeYear = run.RelativeYear,
            User = run.CreatedBy,
            DefaultParameters = await parameterService.GetDefaultParameters(run.Id)
        };

        await resultBuilder.BuildAsync(runContext, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task DeleteExistingRunResultsAsync(int runId, CancellationToken cancellationToken)
    {
        await dbContext.ProducerFeeDetails.Where(d => dbContext.ProducerDisposalFee.Any(f => f.Id == d.ProducerFeesId && f.CalculatorRunId == runId)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.ProducerSelfManagedConsumerWaste.Where(p => dbContext.SelfManagedConsumerWaste.Any(s => s.Id == p.SmcwId && s.CalculatorRunId == runId)).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TransformProjectedH1.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TransformProjectedH2.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TransformScaled.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TransformPartial.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.LapcapData.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.LateReportingTonnage.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.ParameterOtherCost.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.OnePlusFourApportionment.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.CancelledProducers.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.LaDisposalCostData.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.CommCost.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.SelfManagedConsumerWaste.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.ModulationResult.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.ProducerDisposalFee.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.CountryApportionment.Where(x => x.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.ProducerMaterialPackaging.Where(x => x.ProducerDetail.CalculatorRunId == runId).ExecuteDeleteAsync(cancellationToken);
    }
}
