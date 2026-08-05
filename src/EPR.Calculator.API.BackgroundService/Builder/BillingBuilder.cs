using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.BackgroundService.Builder;

public interface IBillingBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

public class BillingBuilder(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader,
    ITelemetryClient telemetryClient,
    ILogger<BillingBuilder> logger
)  : IBillingBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration(nameof(BillingBuilder), () => BuildResult(runContext, cancellationToken));

    private async Task<CalcResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var result = CalcResult.Empty;

        result.CalcResultDetail = await logger.LogDuration(
            () => calcResultDetailBuilder.ConstructAsync(runContext),
            nameof(calcResultDetailBuilder));

        result.CalcResultLapcapData = await logger.LogDuration(
            () => calcResultReader.ReadLapcapData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLapcapData));

        result.CalcResultLateReportingTonnageData = await logger.LogDuration(
            () => calcResultReader.ReadLateReportingTonnage(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLateReportingTonnage));

        result.CalcResultParameterOtherCost = await logger.LogDuration(
            () => calcResultReader.ReadParameterOtherCost(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadParameterOtherCost));

        result.CalcResultOnePlusFourApportionment = await logger.LogDuration(
            () => calcResultReader.ReadOnePlusFourApportionment(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadOnePlusFourApportionment));

        if (runContext.RequiresModulation)
        {
            result.CalcResultProjectedProducers.H1ProjectedProducers = (await logger.LogDuration(
                () => calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadH1ProjectedData))).ToImmutableList();

            result.CalcResultProjectedProducers.H2ProjectedProducers = (await logger.LogDuration(
                () => calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadH2ProjectedData))).ToImmutableList();
        }

        if (runContext.RequiresScaling)
        {
            result.CalcResultScaledupProducers.ScaledupProducers = (await logger.LogDuration(
                () => calcResultReader.ReadScaledData(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadScaledData))).ToImmutableList();
        }

        result.CalcResultPartialObligations.PartialObligations = (await logger.LogDuration(
            () => calcResultReader.ReadPartialData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadPartialData))).ToImmutableList();

        result.CalcResultRejectedProducers = await logger.LogDuration(
            () => rejectedProducersBuilder.ConstructAsync(runContext),
            nameof(rejectedProducersBuilder));

        result.CalcResultCancelledProducers = (await logger.LogDuration(
            () => calcResultReader.ReadCancelledProducers(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadCancelledProducers))).ToList();

        result.Smcw = await logger.LogDuration(
            () => calcResultReader.ReadSmcw(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadSmcw));

        result.CalcResultLaDisposalCostData = await logger.LogDuration(
            () => calcResultReader.ReadLaDisposalCostData(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadLaDisposalCostData));

        result.CalcResultCommsCostReportDetail = await logger.LogDuration(
            () => calcResultReader.ReadCommsCost(runContext.RunId, cancellationToken),
            nameof(calcResultReader.ReadCommsCost));

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await logger.LogDuration(
                () => calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadModulationResult));
        }

        result.ProducerFees = await logger.LogDuration(
                () => calcResultReader.ReadProducerFees(runContext.RunId, cancellationToken),
                nameof(calcResultReader.ReadProducerFees));

        return result;
    }
}
