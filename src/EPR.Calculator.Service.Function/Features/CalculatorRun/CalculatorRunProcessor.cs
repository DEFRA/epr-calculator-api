using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;
using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun;

public interface ICalculatorRunProcessor
{
    Task<RunResult> Process(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunProcessor(
    ApplicationDBContext dbContext,
    ICalculatorRunDataInitializer dataInitializer,
    ICalculatorRunFinalizer finalizer,
    ICalcResultBuilder resultBuilder,
    ICalculatorFileGenerator fileGenerator,
    ILogger<CalculatorRunProcessor> logger)
    : ICalculatorRunProcessor
{
    public async Task<RunResult> Process(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            await SaveRunningRunStatus(runContext, cancellationToken);

            // ⚠️ Database mutations can happen throughout many of these calls
            // This behaviour is different to BillingRunProcessor (which only does so in the finalizer).
            await dataInitializer.Initialize(runContext, cancellationToken);

            // This reads the required data to memory and builds the CalcResult object.
            // For CalculatorRunContext, it causes external state mutations.
            var calcResult = await resultBuilder.BuildAsync(runContext);

            // This writes the CSV/JSON files to blob storage.
            // It does not mutate the database state (handled in the finalizer).
            var exportResult = await fileGenerator.SerializeAndExport(runContext, calcResult, cancellationToken);

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, calcResult, exportResult, cancellationToken);

            return new CalculatorRunResult
            {
                ExportResult = exportResult
            };
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
