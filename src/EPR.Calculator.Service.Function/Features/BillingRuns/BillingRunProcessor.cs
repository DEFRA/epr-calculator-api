using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRuns.Outputs;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Features.BillingRuns;

public interface IBillingRunProcessor
{
    Task<RunResult> Process(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingRunProcessor(
    IBillingBuilder resultBuilder,
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
            var calcResult = await resultBuilder.BuildAsync(runContext, cancellationToken);

            // Filter by accepted/rejected producer ids
            var filteredCalcResult = GetFilteredCalcResult(calcResult, runContext);

            // This writes the CSV/JSON files to blob storage.
            // It does not mutate the database state (handled in the finalizer).
            var exportResult = await fileGenerator.SerializeAndExport(runContext, filteredCalcResult, cancellationToken);

            // This mutates the state of various database entities to reflect the completed run.
            await finalizer.FinalizeAsCompleted(runContext, filteredCalcResult, exportResult, cancellationToken);

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
            CalcResultProjectedProducers = calcResult.CalcResultProjectedProducers with
            {
                H1ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H1ProjectedProducers, p => p.ProducerId),
                H2ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H2ProjectedProducers, p => p.ProducerId)
            },
            CalcResultScaledupProducers = calcResult.CalcResultScaledupProducers with { ScaledupProducers = FilterAccepted(calcResult.CalcResultScaledupProducers.ScaledupProducers, p => p.ProducerId) },
            CalcResultPartialObligations = calcResult.CalcResultPartialObligations with { PartialObligations = FilterAccepted(calcResult.CalcResultPartialObligations.PartialObligations, p => p.ProducerId) },
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
