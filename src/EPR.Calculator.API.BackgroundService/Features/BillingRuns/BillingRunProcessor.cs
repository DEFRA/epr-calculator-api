using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns;

public interface IBillingRunProcessor
{
    Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    IBillingBuilder resultBuilder,
    IBillingFileGenerator fileGenerator,
    IBillingRunFinalizer finalizer,
    ICalcResultReader calcResultReader,
    ILogger<BillingRunProcessor> logger
) : IBillingRunProcessor
{
    [ActivityMetric(nameof(Metrics.TotalDuration), threshold: "00:15:00")]
    public async Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            // Reads the required data to memory, builds the CalcResult, and filters it to the accepted
            // producers. The unfiltered build result is not held in a local so it can be collected as
            // soon as the filter has copied what it needs. (No external state mutations here.)
            var filteredCalcResult = GetFilteredCalcResult(await resultBuilder.BuildAsync(runContext, cancellationToken), runContext);

            // The per-producer fee rows are streamed off the reader (accepted producers only) rather than
            // materialised on filteredCalcResult.ProducerFees.Details, so the whole fee graph is never held.
            // The finalizer only needs each producer's Level-1 BillingInstruction back out of that graph,
            // so it's captured here as a side effect of the CSV/JSON export's own pass over the stream -
            // that avoids a third full pass over the ~10^4-row, owned-JSON fee graph just to re-read it.
            var level1BillingInstructions = new Dictionary<int, BillingInstruction?>();

            var acceptedFeeDetails = calcResultReader
                .StreamProducerFeeDetails(runContext.RunId)
                .Where(d => runContext.AcceptedProducerIds.Contains(d.ProducerId))
                .Select(d =>
                {
                    if (d.Level == CommonConstants.LevelOne.ToString())
                        level1BillingInstructions[d.ProducerId] = d.BillingInstruction;
                    return d;
                });

            // This writes the CSV/JSON files to blob storage.
            // It does not mutate the database state (handled in the finalizer).
            var exportResult = await fileGenerator.SerializeAndExport(runContext, filteredCalcResult, acceptedFeeDetails, cancellationToken);

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, filteredCalcResult, exportResult, level1BillingInstructions, cancellationToken);

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

    private static CalcResult GetFilteredCalcResult(CalcResult calcResult, BillingRunContext runContext)
    {
        ImmutableList<T> FilterAccepted<T>(IEnumerable<T> producers, Func<T, int> producerId) =>
            producers.Where(producer => runContext.AcceptedProducerIds.Contains(producerId(producer))).ToImmutableList();

        var rejectedProducerIds = calcResult.CalcResultRejectedProducers.Select(r => r.ProducerId).ToHashSet();

        return calcResult with
        {
            CalcResultProjectedProducers = new CalcResultProjectedProducers { H1ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H1ProjectedProducers, p => p.ProducerId), H2ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H2ProjectedProducers, p => p.ProducerId) },
            CalcResultScaledupProducers  = new CalcResultScaledupProducers  { ScaledupProducers    = FilterAccepted(calcResult.CalcResultScaledupProducers.ScaledupProducers, p => p.ProducerId) },
            CalcResultPartialObligations = new CalcResultPartialObligations { PartialObligations   = FilterAccepted(calcResult.CalcResultPartialObligations.PartialObligations, p => p.ProducerId) },
            ProducerFees = new ProducerFees
            {
                CalculatorRunId = runContext.RunId,
                Details         = FilterAccepted(calcResult.ProducerFees.Details, p => p.FeeDetail.ProducerId),
                Total           = BillingTotal(calcResult.ProducerFees.Total)
            },
            CalcResultCancelledProducers = calcResult.CalcResultCancelledProducers.Where(p => !rejectedProducerIds.Contains(p.ProducerId)).ToList()
        };
    }

    private static FeeDetail BillingTotal(FeeDetail total) => new()
    {
        ProducerId                               = 0,
        SubsidiaryId                             = string.Empty,
        ProducerName                             = string.Empty,
        TradingName                              = string.Empty,
        StatusCode                               = string.Empty,
        JoinerDate                               = string.Empty,
        LeaverDate                               = CommonConstants.Totals,
        TonnageChangeCount                       = string.Empty,
        TonnageChangeAdvice                      = string.Empty,
        BillingInstruction                       = new BillingInstruction { SuggestedBillingInstruction = string.Empty },
        LADisposalCostsSection1                  = total.LADisposalCostsSection1,
        CommsCostsSection2a                      = total.CommsCostsSection2a,
        CommsCostsSection2b                      = total.CommsCostsSection2b,
        CommsCostsSection2c                      = total.CommsCostsSection2c,
        SaOperatingCostsSection3                 = total.SaOperatingCostsSection3,
        LaDataPrepSection4                       = total.LaDataPrepSection4,
        SaSetupCostsSection5                     = total.SaSetupCostsSection5,
        TotalOnePlus2A2B2CWithBadDebtPercentage  = total.TotalOnePlus2A2B2CWithBadDebtPercentage
    };
}
