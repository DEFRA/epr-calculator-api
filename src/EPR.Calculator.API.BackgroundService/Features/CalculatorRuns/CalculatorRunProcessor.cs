using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface ICalculatorRunProcessor
{
    Task<RunResult> Process(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunProcessor(
    ApplicationDBContext dbContext,
    ICalculatorRunDataInitializer dataInitializer,
    ICalculatorRunFinalizer finalizer,
    IResultBuilder resultBuilder,
    ILogger<CalculatorRunProcessor> logger)
    : ICalculatorRunProcessor
{
    [ActivityMetric(nameof(Metrics.TotalDuration), threshold: "00:15:00")]
    public async Task<RunResult> Process(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            // ⚠️ Database mutations can happen throughout many of these calls
            // This behaviour is different to BillingRunProcessor (which only does so in the finalizer).
            await SaveRunningRunStatus(runContext, cancellationToken);

            // This streams required data from the common data API and transposes it into the paycal database.
            await dataInitializer.Initialize(runContext, cancellationToken);

            // This reads the required data to memory and builds the CalcResult object.
            // For CalculatorRunContext, it causes external state mutations.
            var calcResult = await resultBuilder.BuildAsync(runContext, cancellationToken);

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, calcResult, cancellationToken);

            return new CalculatorRunResult();
        }
        catch (Exception ex)
        {
            // ⚠️ For calculator run exceptions, the database state may have mutated.
            // It IS NOT safe to retry in this scenario.
            var type = ex is OperationCanceledException ? "Cancellation" : "Unhandled exception";
            logger.LogError(ex, "Calculation run failed due to {ExceptionType}", type);
            await finalizer.FinalizeAsErrored(runContext, cancellationToken);

            return new BadResult
            {
                Exception = ex
            };
        }
    }

    private async Task SaveRunningRunStatus(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.RUNNINGID;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
