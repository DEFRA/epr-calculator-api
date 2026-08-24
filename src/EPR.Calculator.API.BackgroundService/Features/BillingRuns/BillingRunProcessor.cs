using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Data;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns;

public interface IBillingRunProcessor
{
    Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    ApplicationDBContext dbContext,
    IBillingRunFinalizer finalizer,
    ILogger<BillingRunProcessor> logger
) : IBillingRunProcessor
{
    [ActivityMetric(nameof(Metrics.TotalDuration), threshold: "00:15:00")]
    public async Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var producerFees = dbContext.ProducerDisposalFee.SingleOrDefault(f => f.CalculatorRunId == runContext.RunId);

            if(producerFees is null)
                throw new InvalidOperationException("ProducerFees cannot be null for billing file run");

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, producerFees, cancellationToken);

            return new BillingRunResult();
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
