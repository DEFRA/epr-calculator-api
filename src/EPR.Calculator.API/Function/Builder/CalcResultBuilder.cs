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
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder;

public interface ICalcResultBuilder
{
    Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto, IImmutableList<MaterialDetail> materials);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultBuilder(
    IParameterService parameterService,
    ICalcResultLapcapDataBuilder lapcapData,
    ICalcResultLateReportingBuilder lateReportingTonnage,
    ICalcResultParameterOtherCostBuilder otherCosts,
    ICalcResultOnePlusFourApportionmentBuilder onePlusFourApportionment,
    ICalcResultCancelledProducersBuilder cancelledProducers,
    IReportedProducerService reportedProducers,
    ICalcResultProjectedProducersBuilder projectedProducers,
    ICalcResultScaledupProducersBuilder scaledUpProducers,
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultCommsCostBuilder commsCosts,
    ICalcRunLaDisposalCostBuilder laDisposalCosts,
    ICalcResultPartialObligationBuilder partialObligations,
    ICalcResultSummaryBuilder summary,
    ICalcResultRejectedProducersBuilder rejectedProducers,
    ICalcResultErrorReportBuilder errorReport,
    IProjectedProducersService projectedProducersService,
    ISelfManagedConsumerWasteService selfManagedConsumerWaste,
    ICalcResultModulationBuilder modulation,
    ILogger<CalcResultBuilder> logger)
    : ICalcResultBuilder
{
    public async Task<CalcResult> BuildAsync(CalcResultsRequestDto resultsRequestDto, IImmutableList<MaterialDetail> materials)
    {
        var result = new CalcResult
        {
            ApplyModulation = resultsRequestDto.RelativeYear.Value >= 2026,
            CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(resultsRequestDto),
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
            CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultCancelledProducers = new CalcResultCancelledProducersResponse(),
            CalcResultRejectedProducers = new List<CalcResultRejectedProducer>()
        };

        var defaultParams = await parameterService.GetDefaultParameters(resultsRequestDto.RunId);

        result.CalcResultLapcapData = await logger.LogDuration(() =>
                lapcapData.ConstructAsync(materials, resultsRequestDto),
            nameof(lapcapData));

        result.CalcResultLateReportingTonnageData = await logger.LogDuration(() =>
                lateReportingTonnage.ConstructAsync(materials, resultsRequestDto),
            nameof(lateReportingTonnage));

        result.CalcResultParameterOtherCost = await logger.LogDuration(() =>
                otherCosts.ConstructAsync(resultsRequestDto),
            nameof(otherCosts));

        result.CalcResultOnePlusFourApportionment = logger.LogDuration(() =>
                onePlusFourApportionment.Construct(resultsRequestDto, result),
            nameof(onePlusFourApportionment));

        result.CalcResultCancelledProducers = await logger.LogDuration(() =>
                cancelledProducers.ConstructAsync(resultsRequestDto, materials),
            nameof(cancelledProducers));

        // ReSharper disable AccessToModifiedClosure - LogDuration always immediately invokes the delegate
        var producers = await reportedProducers.GetProducers(resultsRequestDto.RunId);

        if (result.ApplyModulation)
        {
            (producers, result.CalcResultProjectedProducers) = logger.LogDuration(() =>
                    projectedProducers.Construct(materials, producers, resultsRequestDto),
                nameof(projectedProducers));
        }

        if (resultsRequestDto.RelativeYear.Value == 2025)
        {
            (producers, result.CalcResultScaledupProducers) = await logger.LogDuration(() =>
                    scaledUpProducers.ConstructAsync(materials, producers, resultsRequestDto),
                nameof(scaledUpProducers));
        }

        (producers, result.CalcResultPartialObligations) = await logger.LogDuration(() =>
                partialObligations.ConstructAsync(materials, producers, resultsRequestDto, result.ApplyModulation),
            nameof(partialObligations));
        // ReSharper restore AccessToModifiedClosure

        if (!resultsRequestDto.IsBillingFile)
        {
            await logger.LogDuration(() =>
                    projectedProducersService.StoreProjectedProducers(resultsRequestDto.RunId, producers),
                nameof(projectedProducersService.StoreProjectedProducers));
        }

        if (resultsRequestDto.IsBillingFile)
        {
            result.CalcResultRejectedProducers = await logger.LogDuration(() =>
                    rejectedProducers.ConstructAsync(resultsRequestDto),
                nameof(rejectedProducers));
        }

        result.Smcw = await logger.LogDuration(() =>
                selfManagedConsumerWaste.Calculate(resultsRequestDto, materials, result.ApplyModulation),
            nameof(selfManagedConsumerWaste));

        result.CalcResultLaDisposalCostData = await logger.LogDuration(() =>
                laDisposalCosts.ConstructAsync(resultsRequestDto, materials, result.CalcResultLapcapData, result.CalcResultLateReportingTonnageData, result.Smcw, result.ApplyModulation),
            nameof(laDisposalCosts));

        result.CalcResultCommsCostReportDetail = await logger.LogDuration(() =>
                commsCosts.ConstructAsync(materials, resultsRequestDto, result.CalcResultOnePlusFourApportionment, result.CalcResultLateReportingTonnageData),
            nameof(commsCosts));

        if (result.ApplyModulation)
        {
            result.CalcResultModulation = await logger.LogDuration(() =>
                    modulation.ConstructAsync(defaultParams, materials, result.CalcResultLaDisposalCostData, result.Smcw),
                nameof(modulation));
        }

        result.CalcResultSummary = await logger.LogDuration(() =>
                summary.ConstructAsync(materials, resultsRequestDto.RunId, resultsRequestDto.RelativeYear, resultsRequestDto.IsBillingFile, result, result.Smcw),
            nameof(summary));

        result.CalcResultErrorReports = logger.LogDuration(() =>
                errorReport.Construct(resultsRequestDto),
            nameof(errorReport));

        return result;
    }
}
