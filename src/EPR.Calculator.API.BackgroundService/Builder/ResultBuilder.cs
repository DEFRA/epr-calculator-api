using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Builder.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Builder.CommsCost;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Builder.Lapcap;
using EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages;
using EPR.Calculator.API.BackgroundService.Builder.Modulation;
using EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.BackgroundService.Builder.ParametersOther;
using EPR.Calculator.API.BackgroundService.Builder.PartialObligations;
using EPR.Calculator.API.BackgroundService.Builder.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.BackgroundService.Builder;

public interface IResultBuilder
{
    Task<CalcResult> BuildAsync(CalculatorRunContext runContext, CancellationToken cancellationToken);
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
    IMaterialService materialService
)  : IResultBuilder
{
    [ActivityMetric(nameof(Metrics.CalcDuration), threshold: "00:00:30")]
    public async Task<CalcResult> BuildAsync(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var result = CalcResult.Empty;
        var materials = await materialService.GetMaterials();

        result.CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext, cancellationToken);

        result.CalcResultLapcapData = await lapcapDataBuilder.ConstructAsync(runContext, materials, cancellationToken);
        await calcResultWriter.StoreLapcapData(runContext.RunId, result.CalcResultLapcapData, cancellationToken);

        result.CalcResultLateReportingTonnageData = lateReportingTonnageBuilder.Construct(runContext, materials);
        await calcResultWriter.StoreLateReportingTonnage(runContext.RunId, result.CalcResultLateReportingTonnageData, cancellationToken);

        result.CalcResultParameterOtherCost = await otherCostsBuilder.ConstructAsync(runContext);
        await calcResultWriter.StoreParameterOtherCost(runContext.RunId, result.CalcResultParameterOtherCost, cancellationToken);

        result.CalcResultOnePlusFourApportionment = onePlusFourApportionmentBuilder.Construct(result);
        await calcResultWriter.StoreOnePlusFourApportionment(runContext.RunId, result.CalcResultOnePlusFourApportionment, cancellationToken);

        result.CalcResultCancelledProducers = await cancelledProducersBuilder.ConstructAsync(runContext, materials);
        await calcResultWriter.StoreCancelledProducers(runContext.RunId, result.CalcResultCancelledProducers, cancellationToken);

        var producers = await reportedProducersService.GetProducers(runContext);

        if (runContext.RequiresModulation)
        {
            (producers, result.CalcResultProjectedProducers) = projectedProducersBuilder.Construct(runContext, materials, producers);
            await calcResultWriter.StoreProjectedH1Data(runContext.RunId, result.CalcResultProjectedProducers.H1ProjectedProducers, cancellationToken);
            await calcResultWriter.StoreProjectedH2Data(runContext.RunId, result.CalcResultProjectedProducers.H2ProjectedProducers, cancellationToken);
        }

        if (runContext.RequiresScaling)
        {
            (producers, result.CalcResultScaledupProducers) = await scaledUpProducersBuilder.ConstructAsync(runContext, materials, producers);
            await calcResultWriter.StoreScaledData(runContext.RunId, result.CalcResultScaledupProducers.ScaledupProducers, cancellationToken);
        }

        (producers, result.CalcResultPartialObligations) = await partialObligationsBuilder.ConstructAsync(runContext, materials, producers);
        await calcResultWriter.StorePartialData(runContext.RunId, result.CalcResultPartialObligations.PartialObligations, cancellationToken);
        await calcResultWriter.StoreProducerMaterialPackaging(producers, cancellationToken);

        result.Smcw = await selfManagedConsumerWasteService.Calculate(runContext, materials);
        await calcResultWriter.StoreSmcw(runContext.RunId, result.Smcw, cancellationToken);

        result.CalcResultLaDisposalCostData = await laDisposalCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw);
        await calcResultWriter.StoreLaDisposalCostData(runContext.RunId, result.CalcResultLaDisposalCostData, cancellationToken);

        result.CalcResultCommsCostReportDetail = await commsCostsBuilder.ConstructAsync(runContext, materials, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData);
        await calcResultWriter.StoreCommsCost(runContext.RunId, result.CalcResultCommsCostReportDetail, cancellationToken);

        if (runContext.RequiresModulation)
        {
            result.CalcResultModulation = await modulationBuilder.ConstructAsync(runContext, materials, result.CalcResultLaDisposalCostData, result.Smcw);
            await calcResultWriter.StoreModulationResult(runContext.RunId, result.CalcResultModulation, cancellationToken);
        }

        result.ProducerFees = await producerFeesBuilder.ConstructAsync(runContext, materials, result, result.Smcw);
        await calcResultWriter.StoreProducerFees(runContext.RunId, result.ProducerFees, cancellationToken);

        result.CalcResultErrorReports = errorReportBuilder.Construct(runContext);

        return result;
    }
}
