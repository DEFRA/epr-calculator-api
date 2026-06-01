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
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface IBillingFileExporter
    {
        string Export(CalcResult calcResult, IImmutableList<MaterialDetail> materials, ImmutableHashSet<int> acceptedProducerIds);
    }

    public class BillingFileExporter : IBillingFileExporter
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporterCsv;
        private readonly ICalcResultDetailExporter resultDetailexporterCsv;
        private readonly ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionmentExporterCsv;
        private readonly ICalcResultLapcapDataExporter lapcapDataExporterCsv;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCostsCsv;
        private readonly ICalcResultLateReportingExporter lateReportingExporterCsv;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporterCsv;
        private readonly ICalcResultPartialObligationsExporter calcResultPartialObligationsExporterCsv;
        private readonly ICalcResultProjectedProducersExporter calcResultProjectedProducersExporterCsv;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporterCsv;
        private readonly ICalcResultModulationExporter modulationExporterCsv;
        private readonly ICalcResultCommsCostExporter commsCostExporterCsv;
        private readonly ICalcResultCancelledProducersExporter calcResultCancelledProducersExporterCsv;
        private readonly ICalcResultRejectedProducersExporter calcResultRejectedProducersExporterCsv;

        [SuppressMessage("Constructor has 8 parameters, which is greater than the 7 authorized.", "S107", Justification = "This is suppressed for now and will be refactored later")]
        public BillingFileExporter(
            ICalcResultLateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            ICalcResultOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultModulationExporter modulationExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ICalcResultPartialObligationsExporter calcResultPartialObligationsExporter,
            ICalcResultProjectedProducersExporter calcResultProjectedProducersExporter,
            ICalcResultLapcapDataExporter lapcapDataExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICalcResultCommsCostExporter commsCostExporter,
            ICalcResultSummaryExporter calcResultSummaryExporter,
            ICalcResultCancelledProducersExporter calcResultCancelledProducersExporter,
            ICalcResultRejectedProducersExporter calcResultRejectedProducersExporter)
        {
            resultDetailexporterCsv = resultDetailexporter;
            onePlusFourApportionmentExporterCsv = onePlusFourApportionmentExporter;
            lateReportingExporterCsv = lateReportingExporter;
            calcResultScaledupProducersExporterCsv = calcResultScaledupProducersExporter;
            calcResultPartialObligationsExporterCsv = calcResultPartialObligationsExporter;
            calcResultProjectedProducersExporterCsv = calcResultProjectedProducersExporter;
            lapcapDataExporterCsv = lapcapDataExporter;
            parameterOtherCostsCsv = parameterOtherCosts;
            calcResultSummaryExporterCsv = calcResultSummaryExporter;
            laDisposalCostExporterCsv = laDisposalCostExporter;
            modulationExporterCsv = modulationExporter;
            commsCostExporterCsv = commsCostExporter;
            calcResultCancelledProducersExporterCsv = calcResultCancelledProducersExporter;
            calcResultRejectedProducersExporterCsv = calcResultRejectedProducersExporter;
        }

        public string Export(CalcResult calcResult, IImmutableList<MaterialDetail> materials, ImmutableHashSet<int> acceptedProducerIds)
        {
            var csvContent = new StringBuilder();
            resultDetailexporterCsv.Export(calcResult.CalcResultDetail, csvContent);

            lapcapDataExporterCsv.Export(calcResult.CalcResultLapcapData, materials, csvContent);

            lateReportingExporterCsv.Export(materials, calcResult.CalcResultLateReportingTonnageData, csvContent);

            parameterOtherCostsCsv.Export(calcResult.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporterCsv.Export(calcResult.CalcResultOnePlusFourApportionment, csvContent);

            commsCostExporterCsv.Export(calcResult.CalcResultCommsCostReportDetail, materials, csvContent);

            laDisposalCostExporterCsv.Export(calcResult.ApplyModulation, materials, calcResult.CalcResultLaDisposalCostData, csvContent);

            if (calcResult.Smcw is not null && calcResult.CalcResultModulation is not null) {
                modulationExporterCsv.Export(calcResult.CalcResultLaDisposalCostData, calcResult.Smcw, calcResult.CalcResultModulation, csvContent);
            }

            calcResultCancelledProducersExporterCsv.Export(calcResult.CalcResultCancelledProducers, csvContent);

            if (calcResult.ApplyModulation)
            {
                var accepted = GetProjectedProducerForExport(calcResult.CalcResultProjectedProducers, acceptedProducerIds);
                calcResultProjectedProducersExporterCsv.Export(accepted, materials, csvContent);
            } else {
                var acceptedProducers = GetScaledUpProducersForExport(calcResult.CalcResultScaledupProducers, acceptedProducerIds);
                calcResultScaledupProducersExporterCsv.Export(acceptedProducers, materials, showTotal : false, csvContent);
            }

            calcResultPartialObligationsExporterCsv.Export(GetPartialObligationsForExport(calcResult.CalcResultPartialObligations, acceptedProducerIds), materials, csvContent, calcResult.ApplyModulation);

            var acceptedCalcResultSummary = GetAcceptedProducersCalcResults(calcResult.CalcResultSummary, acceptedProducerIds);

            calcResultSummaryExporterCsv.Export(acceptedCalcResultSummary, csvContent, calcResult.ApplyModulation);

            csvContent = ResetTotals(csvContent.ToString());

            calcResultRejectedProducersExporterCsv.Export(calcResult.CalcResultRejectedProducers, csvContent);

            return csvContent.ToString();
        }

        private CalcResultSummary GetAcceptedProducersCalcResults(CalcResultSummary calcResultSummary, ImmutableHashSet<int> acceptedProducerIds)
        {
            return new CalcResultSummary
            {
                BadDebtProvisionFor1 = calcResultSummary.BadDebtProvisionFor1,
                BadDebtProvisionFor2A = calcResultSummary.BadDebtProvisionFor2A,
                BadDebtProvisionTitleSection3 = calcResultSummary.BadDebtProvisionTitleSection3,
                ColumnHeaders = calcResultSummary.ColumnHeaders,
                CommsCostHeaderBadDebtProvisionFor2bTitle = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle,
                CommsCostHeaderWithBadDebtFor2bTitle = calcResultSummary.CommsCostHeaderWithBadDebtFor2bTitle,
                CommsCostHeaderWithoutBadDebtFor2bTitle = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle,
                LaDataPrepCostsBadDebtProvisionTitleSection4 = calcResultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4,
                LaDataPrepCostsTitleSection4 = calcResultSummary.LaDataPrepCostsTitleSection4,
                LaDataPrepCostsWithBadDebtProvisionTitleSection4 = calcResultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4,
                MaterialBreakdownHeaders = calcResultSummary.MaterialBreakdownHeaders,
                NotesHeader = calcResultSummary.NotesHeader,
                ProducerDisposalFees = GetAcceptedProducerDisposalFees(calcResultSummary.ProducerDisposalFees, acceptedProducerIds),
                ProducerDisposalFeesHeaders = calcResultSummary.ProducerDisposalFeesHeaders,
                ResultSummaryHeader = calcResultSummary.ResultSummaryHeader,
                SaOperatingCostsWithTitleSection3 = calcResultSummary.SaOperatingCostsWithTitleSection3,
                SaOperatingCostsWoTitleSection3 = calcResultSummary.SaOperatingCostsWoTitleSection3,
                SaSetupCostsBadDebtProvisionTitleSection5 = calcResultSummary.SaSetupCostsBadDebtProvisionTitleSection5,
                SaSetupCostsTitleSection5 = calcResultSummary.SaSetupCostsTitleSection5,
                SaSetupCostsWithBadDebtProvisionTitleSection5 = calcResultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5,
                TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = calcResultSummary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A,
                TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = calcResultSummary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A,
                TotalFeeforLADisposalCostswithBadDebtprovision1 = calcResultSummary.TotalFeeforLADisposalCostswithBadDebtprovision1,
                TotalFeeforLADisposalCostswoBadDebtprovision1 = calcResultSummary.TotalFeeforLADisposalCostswoBadDebtprovision1,
                TotalOnePlus2A2B2CFeeWithBadDebtProvision = calcResultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision,
                TwoCBadDebtProvision = calcResultSummary.TwoCBadDebtProvision,
                TwoCCommsCostsByCountryWithBadDebtProvision = calcResultSummary.TwoCCommsCostsByCountryWithBadDebtProvision,
                TwoCCommsCostsByCountryWithoutBadDebtProvision = calcResultSummary.TwoCCommsCostsByCountryWithoutBadDebtProvision,
            };
        }

        private IEnumerable<CalcResultSummaryProducerDisposalFees> GetAcceptedProducerDisposalFees(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            ImmutableHashSet<int> acceptedProducerIds)
        {
            var acceptedProducerFees = producerDisposalFees.Where(
                x => acceptedProducerIds.Contains(x.ProducerIdInt)
                || x.ProducerIdInt == 0).ToList();

            acceptedProducerFees.ForEach(x =>
            {
                if (x.isOverallTotalRow)
                {
                    x.ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
                    x.ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
                    ResetObjectUtil.ResetObject(x);
                }
            });
            return acceptedProducerFees;
        }

        public CalcResultScaledupProducers GetScaledUpProducersForExport(
            CalcResultScaledupProducers producers,
            ImmutableHashSet<int> acceptedProducerIds)
        {
            return new CalcResultScaledupProducers
            {
                ScaledupProducers = GetScaledupProducers(producers.ScaledupProducers, acceptedProducerIds)
            };
        }

        private static ImmutableList<CalcResultScaledupProducer> GetScaledupProducers(
            IReadOnlyList<CalcResultScaledupProducer>? scaledupProducers,
            ImmutableHashSet<int> acceptedProducerIds)
        {
            return scaledupProducers?
                .Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0) // TODO why would ProducerId ever be 0?
                .ToImmutableList() ?? [];
        }

        public CalcResultPartialObligations GetPartialObligationsForExport(
            CalcResultPartialObligations producers,
            ImmutableHashSet<int> acceptedProducerIds)
        {
            var acceptedProducers = producers.PartialObligations?
                .Where(x => acceptedProducerIds.Contains(x.ProducerId) || x.ProducerId == 0)
                .ToImmutableList() ?? [];

            return new CalcResultPartialObligations
            {
                PartialObligations = acceptedProducers
            };
        }

        private CalcResultProjectedProducers GetProjectedProducerForExport(
            CalcResultProjectedProducers producers,
            ImmutableHashSet<int> acceptedProducerIds)
        {
            var isAccepted = (int producerId) => acceptedProducerIds.Contains(producerId) || producerId == 0;
            var acceptedH2Producers = producers.H2ProjectedProducers?.Where(x => isAccepted(x.ProducerId)).ToImmutableList() ?? ImmutableList<CalcResultH2ProjectedProducer>.Empty;
            var acceptedH1Producers = producers.H1ProjectedProducers?.Where(x => isAccepted(x.ProducerId)).ToImmutableList() ?? ImmutableList<CalcResultH1ProjectedProducer>.Empty;

            return new CalcResultProjectedProducers
            {
                H2ProjectedProducers = acceptedH2Producers,
                H1ProjectedProducers = acceptedH1Producers,
            };
        }

        private StringBuilder ResetTotals(string sb)
        {
            var idx = sb.LastIndexOf(CommonConstants.Totals);
            if (idx < 0)
                return new StringBuilder(sb);
            var exceptTotals = sb.Substring(0, idx + CommonConstants.Totals.Length + 2);
            return new StringBuilder().Append(exceptTotals);
        }

    }
}
