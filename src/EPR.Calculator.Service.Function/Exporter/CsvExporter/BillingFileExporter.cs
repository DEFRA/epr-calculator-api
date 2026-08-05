using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface IBillingFileExporter
{
    Task<string> Export(BillingRunContext runContext, CalcResult calcResult);
}

[SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
public class BillingFileExporter(
    IMaterialService materialService,
    ICalcResultLateReportingExporter lateReportingExporter,
    ICalcResultDetailExporter resultDetailExporter,
    ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
    ICalcResultLaDisposalCostExporter laDisposalCostExporter,
    ICalcResultModulationExporter modulationExporter,
    ICalcResultScaledupProducersExporter scaledUpProducersExporter,
    ICalcResultPartialObligationsExporter partialObligationsExporter,
    ICalcResultProjectedProducersExporter projectedProducersExporter,
    ICalcResultLapcapDataExporter lapcapDataExporter,
    ICalcResultParameterOtherCostExporter parameterOtherCostExporter,
    ICalcResultCommsCostExporter commsCostExporter,
    IProducerFeesExporter producerFeesExporter,
    ICalcResultCancelledProducersExporter cancelledProducersExporter,
    ICalcResultRejectedProducersExporter rejectedProducersExporter,
    ILogger<BillingFileExporter> logger
)  : IBillingFileExporter
{
    public async Task<string> Export(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        logger.LogDuration(
            () => resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent),
            nameof(resultDetailExporter)
        );

        logger.LogDuration(
            () => lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent),
            nameof(lapcapDataExporter)
        );

        logger.LogDuration(
            () => lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent),
            nameof(lateReportingExporter)
        );

        logger.LogDuration(
            () => parameterOtherCostExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent),
            nameof(parameterOtherCostExporter)
        );

        logger.LogDuration(
            () => onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent),
            nameof(onePlusFourApportionmentExporter)
        );

        logger.LogDuration(
            () => commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent),
            nameof(commsCostExporter)
        );

        logger.LogDuration(
            () => laDisposalCostExporter.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent),
            nameof(laDisposalCostExporter)
        );

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            logger.LogDuration(
                () => modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent),
                nameof(modulationExporter)
            );

        logger.LogDuration(
            () => cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent),
            nameof(cancelledProducersExporter)
        );

        if (runContext.RequiresModulation)
        {
            logger.LogDuration(
                () => projectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, materials, csvContent),
                nameof(projectedProducersExporter)
            );
        }
        else
        {
            logger.LogDuration(
                () => scaledUpProducersExporter.Export(calcResult.CalcResultScaledupProducers, materials, false, csvContent),
                nameof(scaledUpProducersExporter)
            );
        }

        logger.LogDuration(
            () => partialObligationsExporter.Export(runContext, calcResult.CalcResultPartialObligations, materials, csvContent),
            nameof(partialObligationsExporter)
        );

        var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
        var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();
        logger.LogDuration(
            () => producerFeesExporter.Export(runContext, calcResult.ProducerFees, materials, scaledupIds, partialIds, csvContent),
            nameof(producerFeesExporter)
        );

        csvContent = ResetTotals(csvContent.ToString());

        logger.LogDuration(
            () => rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent),
            nameof(rejectedProducersExporter)
        );

        return csvContent.ToString();
    }

    private static StringBuilder ResetTotals(string sb)
    {
        var idx = sb.LastIndexOf(CommonConstants.Totals, StringComparison.Ordinal);
        if (idx < 0)
            return new StringBuilder(sb);
        var exceptTotals = sb.Substring(0, idx + CommonConstants.Totals.Length + 2);
        return new StringBuilder().Append(exceptTotals);
    }
}
