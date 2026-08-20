using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.BackgroundService.Builder;

public interface IBillingBuilder
{
    Task<CalcResult> BuildAsync(BillingRunContext runContext, CancellationToken cancellationToken);
}

public class BillingBuilder(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader
)  : IBillingBuilder
{
    [ActivityMetric(nameof(Metrics.CalcDuration), threshold: "00:00:30")]
    public async Task<CalcResult> BuildAsync(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        var result = CalcResult.Empty;
        result.CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext, cancellationToken);
        result.CalcResultLapcapData = await calcResultReader.ReadLapcapData(runContext.RunId, cancellationToken);
        result.CalcResultLateReportingTonnageData = await calcResultReader.ReadLateReportingTonnage(runContext.RunId, cancellationToken);
        result.CalcResultParameterOtherCost = await calcResultReader.ReadParameterOtherCost(runContext.RunId, cancellationToken);
        result.CalcResultOnePlusFourApportionment = await calcResultReader.ReadOnePlusFourApportionment(runContext.RunId, cancellationToken);

        if (runContext.RequiresModulation)
        {
            result.CalcResultProjectedProducers.H1ProjectedProducers = await calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken);
            result.CalcResultProjectedProducers.H2ProjectedProducers = await calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken);
        }

        if (runContext.RequiresScaling)
            result.CalcResultScaledupProducers.ScaledupProducers = await calcResultReader.ReadScaledData(runContext.RunId, cancellationToken);

        result.CalcResultPartialObligations.PartialObligations = await calcResultReader.ReadPartialData(runContext.RunId, cancellationToken);
        result.CalcResultRejectedProducers = await rejectedProducersBuilder.ConstructAsync(runContext, cancellationToken);
        result.CalcResultCancelledProducers = await calcResultReader.ReadCancelledProducers(runContext.RunId, cancellationToken);
        result.Smcw = await calcResultReader.ReadSmcw(runContext.RunId, cancellationToken);
        result.CalcResultLaDisposalCostData = await calcResultReader.ReadLaDisposalCostData(runContext.RunId, cancellationToken);
        result.CalcResultCommsCostReportDetail = await calcResultReader.ReadCommsCost(runContext.RunId, cancellationToken);

        if (runContext.RequiresModulation)
            result.CalcResultModulation = await calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken);

        result.ProducerFees = await calcResultReader.ReadProducerFees(runContext.RunId, cancellationToken);

        return result;
    }
}
