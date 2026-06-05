using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter;

public interface ICalcResultsExporter
{
    Task<string> Export(CalculatorRunContext runContext, CalcResult calcResult);
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
    ICalcResultSummaryExporter summaryExporter,
    ICalcResultCancelledProducersExporter cancelledProducersExporter,
    ICalcResultErrorReportExporter calcResultErrorReportExporter
)  : ICalcResultsExporter
{
    public async Task<string> Export(CalculatorRunContext runContext, CalcResult calcResult)
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

        if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null) {
            modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);
        }

        cancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

        if (runContext.RequiresModulation)
            projectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, materials, csvContent);
        else
            scaledUpProducersExporter.Export(calcResult.CalcResultScaledupProducers, materials, showTotal : true, csvContent);

        partialObligationsExporter.Export(runContext, calcResult.CalcResultPartialObligations, materials, csvContent);
        summaryExporter.Export(runContext, calcResult.CalcResultSummary, materials, csvContent);
        calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);

        return csvContent.ToString();
    }
}
