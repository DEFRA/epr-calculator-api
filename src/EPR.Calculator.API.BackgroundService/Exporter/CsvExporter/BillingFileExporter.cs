using System.Diagnostics.CodeAnalysis;
using System.Text;
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
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;

public interface IBillingFileExporter
{
    Task Export(BillingRunContext runContext, CalcResult calcResult, TextWriter writer, IEnumerable<FeeDetail> producerFeeDetails);
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
    public async Task Export(BillingRunContext runContext, CalcResult calcResult, TextWriter writer, IEnumerable<FeeDetail> producerFeeDetails)
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

        // The producer fee rows are the bulk, so they stream straight to the writer. For a billing run
        // ProducerFeesExporter emits only the identity columns of the overall-total row (ending
        // "...,\"Totals\",") - this is what the old string-surgery ResetTotals step used to truncate to.
        await FlushAsync(writer, csvContent);
        await producerFeesExporter.Export(runContext, calcResult.ProducerFees, materials, scaledupIds, partialIds, writer, producerFeeDetails);

        rejectedProducersExporter.Export(calcResult.CalcResultRejectedProducers, csvContent);
        await FlushAsync(writer, csvContent);
    }

    private static async Task FlushAsync(TextWriter writer, StringBuilder buffer)
    {
        foreach (var chunk in buffer.GetChunks())
            await writer.WriteAsync(chunk);
        buffer.Clear();
    }
}
