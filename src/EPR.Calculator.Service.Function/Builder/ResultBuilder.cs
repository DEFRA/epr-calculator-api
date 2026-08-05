using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface IResultBuilder
{
    Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class ResultBuilder(
    ICalcResultLapcapDataBuilder lapcapDataBuilder,
    ICalcResultLateReportingBuilder lateReportingTonnageBuilder,
    ICalcResultParameterOtherCostBuilder otherCostsBuilder,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionmentBuilder,
    ICalcResultCancelledProducersBuilder cancelledProducersBuilder,
    IReportedProducerService reportedProducersService,
    ICalcResultProjectedProducersBuilder projectedProducersBuilder,
    ICalcResultScaledupProducersBuilder scaledUpProducersBuilder,
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultCommsCostBuilder commsCostsBuilder,
    ICalcRunLaDisposalCostBuilder laDisposalCostsBuilder,
    ICalcResultPartialObligationBuilder partialObligationsBuilder,
    IProducerFeesBuilder producerFeesBuilder,
    ICalcResultErrorReportBuilder errorReportBuilder,
    ISelfManagedConsumerWasteService selfManagedConsumerWasteService,
    ICalcResultModulationBuilder modulationBuilder,
    ICalcResultWriter calcResultWriter,
    IMaterialService materialService,
    ITelemetryClient telemetryClient,
    ILogger<ResultBuilder> logger
)  : IResultBuilder
{
    public Task<CalcResult> BuildAsync(RunContext runContext, CancellationToken cancellationToken) =>
        telemetryClient.TrackDuration(nameof(ResultBuilder), () => BuildResult(runContext, cancellationToken));

    private async Task<CalcResult> BuildResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var result = CalcResult.Empty;
        var materials = await materialService.GetMaterials();

        result.CalcResultDetail = await logger.LogDuration(
            () => calcResultDetailBuilder.ConstructAsync(runContext),
            nameof(calcResultDetailBuilder));

        result.CalcResultLapcapData = await logger.LogDuration(
            () => lapcapDataBuilder.ConstructAsync(runContext, materials),
            nameof(lapcapDataBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLapcapData(runContext.RunId, result.CalcResultLapcapData, cancellationToken),
            nameof(calcResultWriter.StoreLapcapData));

        result.CalcResultLateReportingTonnageData = await logger.LogDuration(
            () => lateReportingTonnageBuilder.ConstructAsync(runContext, materials),
            nameof(lateReportingTonnageBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLateReportingTonnage(runContext.RunId, result.CalcResultLateReportingTonnageData, cancellationToken),
            nameof(calcResultWriter.StoreLateReportingTonnage));

        result.CalcResultParameterOtherCost = await logger.LogDuration(
            () => otherCostsBuilder.ConstructAsync(runContext),
            nameof(otherCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreParameterOtherCost(runContext.RunId, result.CalcResultParameterOtherCost, cancellationToken),
            nameof(calcResultWriter.StoreParameterOtherCost));

        result.CalcResultOnePlusFourApportionment = logger.LogDuration(
            () => onePlusFourApportionmentBuilder.Construct(result),
            nameof(onePlusFourApportionmentBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreOnePlusFourApportionment(runContext.RunId, result.CalcResultOnePlusFourApportionment, cancellationToken),
            nameof(calcResultWriter.StoreOnePlusFourApportionment));

        result.CalcResultCancelledProducers = await logger.LogDuration(
            () => cancelledProducersBuilder.ConstructAsync(runContext, materials),
            nameof(cancelledProducersBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreCancelledProducers(runContext.RunId, result.CalcResultCancelledProducers, cancellationToken),
            nameof(calcResultWriter.StoreCancelledProducers));

        var producers = await reportedProducersService.GetProducers(runContext);

        if (runContext.RequiresModulation)
        {
            (producers, result.CalcResultProjectedProducers) = logger.LogDuration(
                () => projectedProducersBuilder.Construct(runContext, materials, producers),
                nameof(projectedProducersBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreProjectedH1Data(runContext.RunId, result.CalcResultProjectedProducers.H1ProjectedProducers, cancellationToken),
                nameof(calcResultWriter.StoreProjectedH1Data));

            await logger.LogDuration(
                () => calcResultWriter.StoreProjectedH2Data(runContext.RunId, result.CalcResultProjectedProducers.H2ProjectedProducers, cancellationToken),
                nameof(calcResultWriter.StoreProjectedH2Data));
        }

        if (runContext.RequiresScaling)
        {
            (producers, result.CalcResultScaledupProducers) = await logger.LogDuration(
                () => scaledUpProducersBuilder.ConstructAsync(runContext, materials, producers),
                nameof(scaledUpProducersBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreScaledData(runContext.RunId, result.CalcResultScaledupProducers.ScaledupProducers, cancellationToken),
                nameof(calcResultWriter.StoreScaledData));
        }

        (producers, result.CalcResultPartialObligations) = await logger.LogDuration(
            () => partialObligationsBuilder.ConstructAsync(runContext, materials, producers),
            nameof(partialObligationsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StorePartialData(runContext.RunId, result.CalcResultPartialObligations.PartialObligations, cancellationToken),
            nameof(calcResultWriter.StorePartialData));

        await logger.LogDuration(
            () => calcResultWriter.StoreProducerMaterialPackaging(producers, cancellationToken),
            nameof(calcResultWriter.StoreProducerMaterialPackaging));

        result.Smcw = await logger.LogDuration(
            () => selfManagedConsumerWasteService.Calculate(runContext, materials),
            nameof(selfManagedConsumerWasteService));

        await logger.LogDuration(
            () => calcResultWriter.StoreSmcw(runContext.RunId, result.Smcw, cancellationToken),
            nameof(calcResultWriter.StoreSmcw));

        result.CalcResultLaDisposalCostData = await logger.LogDuration(
            () => laDisposalCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw),
            nameof(laDisposalCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreLaDisposalCostData(runContext.RunId, result.CalcResultLaDisposalCostData, cancellationToken),
            nameof(calcResultWriter.StoreLaDisposalCostData));

        result.CalcResultCommsCostReportDetail = await logger.LogDuration(
            () => commsCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData),
            nameof(commsCostsBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreCommsCost(runContext.RunId, result.CalcResultCommsCostReportDetail, cancellationToken),
            nameof(calcResultWriter.StoreCommsCost));

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await logger.LogDuration(
                () => modulationBuilder.ConstructAsync(runContext, materials, result.CalcResultLaDisposalCostData, result.Smcw),
                nameof(modulationBuilder));

            await logger.LogDuration(
                () => calcResultWriter.StoreModulationResult(runContext.RunId, result.CalcResultModulation, cancellationToken),
                nameof(calcResultWriter.StoreModulationResult));
        }

        result.ProducerFees = await logger.LogDuration(
            () => producerFeesBuilder.ConstructAsync(runContext, materials, result, result.Smcw),
            nameof(producerFeesBuilder));

        await logger.LogDuration(
            () => calcResultWriter.StoreProducerFees(runContext.RunId, result.ProducerFees, cancellationToken),
            nameof(calcResultWriter.StoreProducerFees));

        result.CalcResultErrorReports = logger.LogDuration(
            () => errorReportBuilder.Construct(runContext),
            nameof(errorReportBuilder));

        return result;
    }
}
