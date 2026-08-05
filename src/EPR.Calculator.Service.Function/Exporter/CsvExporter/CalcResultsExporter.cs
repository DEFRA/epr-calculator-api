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
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Validation;
using EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Logging;
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
    IProducerFeesExporter producerFeesExporter,
    ICalcResultCancelledProducersExporter cancelledProducersExporter,
    ICalcResultErrorReportExporter calcResultErrorReportExporter,
    ICalcResultValidationExporter validationExporter,
    ILogger<CalcResultsExporter> logger
)  : ICalcResultsExporter
{
    public async Task<string> Export(CalculatorRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var csvContent = new StringBuilder();

        logger.LogDuration(
            () => resultDetailExporter.Export(calcResult.CalcResultDetail, csvContent),
            nameof(resultDetailExporter)
        );

        logger.LogDuration(
            () => validationExporter.ExportWarnings(calcResult, csvContent),
            nameof(validationExporter)
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
            () => parameterOtherCostsExporter.Export(calcResult.CalcResultParameterOtherCost, csvContent),
            nameof(parameterOtherCostsExporter)
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
        {
            logger.LogDuration(
                () => modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent),
                nameof(modulationExporter)
            );
        }

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
                () => scaledUpProducersExporter.Export(calcResult.CalcResultScaledupProducers, materials, showTotal : true, csvContent),
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

        logger.LogDuration(
            () => calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent),
            nameof(calcResultErrorReportExporter)
        );

        return csvContent.ToString();
    }
}
