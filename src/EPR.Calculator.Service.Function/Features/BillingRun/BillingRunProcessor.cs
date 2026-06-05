using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRun.Outputs;
using EPR.Calculator.Service.Function.Features.Common;

namespace EPR.Calculator.Service.Function.Features.BillingRun;

public interface IBillingRunProcessor
{
    Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    ICalcResultBuilder resultBuilder,
    IBillingFileGenerator fileGenerator,
    IBillingRunFinalizer finalizer,
    ILogger<BillingRunProcessor> logger)
    : IBillingRunProcessor
{
    public async Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            // This reads the required data to memory and builds the CalcResult object.
            // For BillingRunContext, it does not cause any external state mutations.
            var calcResult = await resultBuilder.BuildAsync(runContext);

            // This writes the CSV/JSON files to blob storage.
            // It does not mutate the database state (handled in the finalizer).
            var exportResult = await fileGenerator.SerializeAndExport(runContext, calcResult, cancellationToken);

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, calcResult, exportResult, cancellationToken);

            return new BillingRunResult
            {
                ExportResult = exportResult
            };
        }
        catch (Exception ex)
        {
            // ⚠️ For billing run exceptions, the database state should NOT have mutated, except for files
            // written to blob storage (which will become orphaned).
            // It should be safe to retry in this scenario.
            var type = ex is OperationCanceledException ? "Cancellation" : "Unhandled exception";
            logger.LogError(ex, "Billing run failed due to {ExceptionType}", type);
            await finalizer.FinalizeAsErrored(runContext, cancellationToken);

            return new BadResult
            {
                Exception = ex
            };
        }
    }
}
