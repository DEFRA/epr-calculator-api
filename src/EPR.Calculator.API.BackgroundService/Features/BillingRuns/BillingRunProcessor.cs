using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns;

public interface IBillingRunProcessor
{
    Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    ApplicationDBContext dbContext,
    ICalcResultReader calcResultReader,
    IBillingRunFinalizer finalizer,
    ILogger<BillingRunProcessor> logger
) : IBillingRunProcessor
{
    [ActivityMetric(nameof(Metrics.TotalDuration), threshold: "00:15:00")]
    public async Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var hasProducerFees = await dbContext.ProducerDisposalFee
                .AsNoTracking()
                .AnyAsync(f => f.CalculatorRunId == runContext.RunId, cancellationToken);

            if (!hasProducerFees)
                throw new InvalidOperationException("ProducerFees cannot be null for billing file run");

            var producerFeeDetails = calcResultReader.StreamProducerFeeDetails(runContext.RunId).ToList();

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, producerFeeDetails, cancellationToken);

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
