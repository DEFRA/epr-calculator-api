using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Detail;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;

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
    ICalcResultParameterOtherCostExporter parameterOtherCostsExporter,
    ICalcResultCommsCostExporter commsCostExporter,
    IProducerFeesExporter producerFeesExporter,
    ICalcResultCancelledProducersExporter cancelledProducersExporter,
    ICalcResultRejectedProducersExporter rejectedProducersExporter
) : IBillingFileExporter
{
    [ActivityMetric(nameof(Metrics.SerializeDuration), threshold: "00:00:30")]
    public async Task<string> Export(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent);
        lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent);
        lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent);
        parameterOtherCostsExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent);
        onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);
        commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);
        laDisposalCostExporter.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent);

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);

        cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, materials, csvContent);

        if (runContext.RequiresModulation)
            projectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, materials, csvContent);
        else
            scaledUpProducersExporter.Export(calcResult.CalcResultScaledupProducers, materials, showTotal: false, csvContent);

        partialObligationsExporter.Export(runContext, calcResult.CalcResultPartialObligations, materials, csvContent);

        var scaledupIds = calcResult.CalcResultScaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
        var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();

        producerFeesExporter.Export(runContext, calcResult.ProducerFees, materials, scaledupIds, partialIds, csvContent);
        csvContent = ResetTotals(csvContent.ToString());
        rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent);

        return csvContent.ToString();
    }

    private static StringBuilder ResetTotals(string sb)
    {
        var idx = sb.LastIndexOf(CommonConstants.Totals, StringComparison.Ordinal);
        if (idx < 0)
            return new StringBuilder(sb);
        var exceptTotals = sb.Substring(startIndex: 0, idx + CommonConstants.Totals.Length + 2);
        return new StringBuilder().Append(exceptTotals);
    }
}
