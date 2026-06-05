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
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface ICalcResultsExporter
    {
        string Export(CalcResult calcResult, IImmutableList<MaterialDetail> materials);
    }

    public class CalcResultsExporter : ICalcResultsExporter
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;
        private readonly ICalcResultLapcapDataExporter lapcapDataExporter;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCosts;
        private readonly ICalcResultLateReportingExporter lateReportingExporter;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter;
        private readonly ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter;
        private readonly ICalcResultProjectedProducersExporter calcResultProjectedProducersExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;
        private readonly ICalcResultModulationExporter modulationExporter;
        private readonly ICalcResultCommsCostExporter commsCostExporter;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter;
        private readonly ICalcResultErrorReportExporter calcResultErrorReportExporter;

        // Suppress SonarQube warning for constructor parameter count
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
        public CalcResultsExporter(
            ICalcResultLateReportingExporter lateReporting,
            ICalcResultDetailExporter resultDetail,
            ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionment,
            ICalcResultLaDisposalCostExporter laDisposalCost,
            ICalcResultModulationExporter modulationExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducers,
            ICalcResultPartialObligationsExporter calcResultPartialObligations,
            ICalcResultProjectedProducersExporter calcResultProjectedProducers,
            ICalcResultLapcapDataExporter lapcapDataExporter,
            ICalcResultParameterOtherCostExporter parameterOt,
            ICalcResultCommsCostExporter commsCost,
            ICalcResultSummaryExporter calcResultSummary,
            ICalcResultCancelledProducersExporter calcResultCancelledProducers,
            ICalcResultErrorReportExporter calcResultErrorReport
        )
        {
            resultDetailexporter = resultDetail;
            onePlusFourApportionmentExporter = onePlusFourApportionment;
            lateReportingExporter = lateReporting;
            calcResultScaledupProducersExporter = calcResultScaledupProducers;
            calcResultPartialObligationsExporter = calcResultPartialObligations;
            calcResultProjectedProducersExporter = calcResultProjectedProducers;
            this.lapcapDataExporter = lapcapDataExporter;
            parameterOtherCosts = parameterOt;
            calcResultSummaryExporter = calcResultSummary;
            laDisposalCostExporter = laDisposalCost;
            this.modulationExporter = modulationExporter;
            commsCostExporter = commsCost;
            calcResultCancelledProducersExporter = calcResultCancelledProducers;
            calcResultErrorReportExporter = calcResultErrorReport;
        }

        public string Export(CalcResult calcResult, IImmutableList<MaterialDetail> materials)
        {
            if (calcResult == null)
            {
                throw new ArgumentNullException(nameof(calcResult), "The calcResult parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            resultDetailexporter.Export(calcResult.CalcResultDetail, csvContent);

            lapcapDataExporter.Export(calcResult.CalcResultLapcapData, materials, csvContent);

            lateReportingExporter.Export(materials, calcResult.CalcResultLateReportingTonnageData, csvContent);

            parameterOtherCosts.Export(calcResult.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporter.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

            commsCostExporter.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);

            laDisposalCostExporter.Export(calcResult.ApplyModulation, materials, calcResult.CalcResultLaDisposalCostData, csvContent);

            if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null) {
                modulationExporter.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);
            }

            calcResultCancelledProducersExporter.Export(calcResult.CalcResultCancelledProducers, csvContent);

            if (calcResult.ApplyModulation)
            {
                calcResultProjectedProducersExporter.Export(calcResult.CalcResultProjectedProducers, materials, csvContent);
            }
            else
            {
                calcResultScaledupProducersExporter.Export(calcResult.CalcResultScaledupProducers, materials, showTotal : true, csvContent);
            }

            calcResultPartialObligationsExporter.Export(calcResult.CalcResultPartialObligations, materials, csvContent, calcResult.ApplyModulation);

            calcResultSummaryExporter.Export(calcResult.CalcResultSummary, csvContent, calcResult.ApplyModulation);

            calcResultErrorReportExporter.Export(calcResult.CalcResultErrorReports, csvContent);

            return csvContent.ToString();
        }
    }
}
