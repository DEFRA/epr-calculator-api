using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Detail;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;

public interface ICalcResultsExporter
{
    Task Export(CalculatorRunContext runContext, CalcResult calcResult, ProducerReportSections producerSections, TextWriter writer, IEnumerable<FeeDetail>? producerFeeDetails = null);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalcResultsExporter(
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
    ICalcResultErrorReportExporter calcResultErrorReportExporter,
    ICalcResultValidationExporter validationExporter
) : ICalcResultsExporter
{
    [ActivityMetric(nameof(Metrics.SerializeDuration), threshold: "00:00:30")]
    public async Task Export(CalculatorRunContext runContext, CalcResult calcResult, ProducerReportSections producerSections, TextWriter writer, IEnumerable<FeeDetail>? producerFeeDetails = null)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent);
        validationExporter.ExportWarnings(calcResult, csvContent);
        lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent);
        lateReportingExporter.Export(calcResult.CalcResultLateReportingTonnageData, materials, csvContent);
        parameterOtherCostsExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent);
        onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);
        commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);
        laDisposalCostExporter.Export(runContext, calcResult.CalcResultLaDisposalCostData, materials, csvContent);

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null)
            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);

        cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, materials, csvContent);

        // Load the large per-producer section, write it, then flush and let it fall out of scope
        // before the long fee-row stream - H1/H2 projected, scaled-up and the fee stream are never
        // all resident at once.
        IReadOnlyList<int> scaledupIds = [];
        if (runContext.RequiresModulation)
        {
            projectedProducersExporter.Export(await producerSections.LoadProjectedProducers(), materials, csvContent);
        }
        else
        {
            var scaledupProducers = await producerSections.LoadScaledupProducers();
            scaledUpProducersExporter.Export(scaledupProducers, materials, showTotal: true, csvContent);
            scaledupIds = scaledupProducers.ScaledupProducers.Select(p => p.ProducerId).ToList();
        }
        await FlushAsync(writer, csvContent);

        partialObligationsExporter.Export(runContext, calcResult.CalcResultPartialObligations, materials, csvContent);
        var partialIds = calcResult.CalcResultPartialObligations.PartialObligations.Select(p => (p.ProducerId, p.SubsidiaryId)).ToList();

        // The producer fee rows are the ~10^4-row bulk, so they stream straight to the writer.
        await FlushAsync(writer, csvContent);
        await producerFeesExporter.Export(runContext, calcResult.ProducerFees, materials, scaledupIds, partialIds, writer, producerFeeDetails);

        calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);
        await FlushAsync(writer, csvContent);
    }

    private static async Task FlushAsync(TextWriter writer, StringBuilder buffer)
    {
        foreach (var chunk in buffer.GetChunks())
            await writer.WriteAsync(chunk);
        buffer.Clear();
    }
}
