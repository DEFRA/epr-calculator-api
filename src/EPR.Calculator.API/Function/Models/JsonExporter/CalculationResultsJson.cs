using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class CalculationResultsJson
    {
        [JsonPropertyName("producerCalculationResultsSummary")]
        public required ProducerCalculationResultsSummary ProducerCalculationResultsSummary { get; set; }

        [JsonPropertyName("producerCalculationResults")]
        public required IEnumerable<CalcSummaryProducerCalculationResults> ProducerCalculationResults { get; set; }

        [JsonPropertyName("producerCalculationResultsTotal")]
        public CalcResultProducerCalculationResultsTotal? ProducerCalculationResultsTotal { get; set; }

        public static CalculationResultsJson From(
            CalcResult result,
            IEnumerable<int> acceptedProducerIds,
            IImmutableList<MaterialDetail> materials)
        {
            return new CalculationResultsJson
            {
                ProducerCalculationResultsSummary = ArrangeSummary(result.CalcResultSummary),
                ProducerCalculationResults = ArrangeProducerCalculationResult(result, acceptedProducerIds, materials),
                ProducerCalculationResultsTotal = ArrangeProducerCalculationResultsTotal(result.CalcResultSummary),
            };
        }

        /// <summary>
        /// Arrange the CalcResultSummary data using the property
        /// names and ordering required for serialisation.
        /// </summary>
        private static ProducerCalculationResultsSummary ArrangeSummary(CalcResultSummary data)
        {
            return new ProducerCalculationResultsSummary
            {
                FeeForLaDisposalCostsWithoutBadDebtprovision1 = CurrencyConverterUtils.ConvertToCurrency(data.TotalFeeforLADisposalCostswoBadDebtprovision1),
                BadDebtProvision1 = CurrencyConverterUtils.ConvertToCurrency(data.BadDebtProvisionFor1),
                FeeForLaDisposalCostsWithBadDebtprovision1 = CurrencyConverterUtils.ConvertToCurrency(data.TotalFeeforLADisposalCostswithBadDebtprovision1),

                FeeForCommsCostsByMaterialWithoutBadDebtprovision2a = CurrencyConverterUtils.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A),
                BadDebtProvision2a = CurrencyConverterUtils.ConvertToCurrency(data.BadDebtProvisionFor2A),
                FeeForCommsCostsByMaterialWitBadDebtprovision2a = CurrencyConverterUtils.ConvertToCurrency(data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A),

                FeeForCommsCostsUkWideWithoutBadDebtprovision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostHeaderWithoutBadDebtFor2bTitle),
                BadDebtProvision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostHeaderBadDebtProvisionFor2bTitle),
                FeeForCommsCostsUkWideWithBadDebtprovision2b = CurrencyConverterUtils.ConvertToCurrency(data.CommsCostHeaderWithBadDebtFor2bTitle),

                FeeForCommsCostsByCountryWithoutBadDebtprovision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCCommsCostsByCountryWithoutBadDebtProvision),
                BadDebtProvision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCBadDebtProvision),
                FeeForCommsCostsByCountryWideWithBadDebtprovision2c = CurrencyConverterUtils.ConvertToCurrency(data.TwoCCommsCostsByCountryWithBadDebtProvision),

                Total12a2b2cWithBadDebt = CurrencyConverterUtils.ConvertToCurrency(data.TotalOnePlus2A2B2CFeeWithBadDebtProvision),

                SaOperatingCostsWithoutBadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsWoTitleSection3),
                BadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.BadDebtProvisionTitleSection3),
                SaOperatingCostsWithBadDebtProvision3 = CurrencyConverterUtils.ConvertToCurrency(data.SaOperatingCostsWithTitleSection3),

                LaDataPrepCostsWithoutBadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepCostsTitleSection4),
                BadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepCostsBadDebtProvisionTitleSection4),
                LaDataPrepCostsWithbadDebtProvision4 = CurrencyConverterUtils.ConvertToCurrency(data.LaDataPrepCostsWithBadDebtProvisionTitleSection4),

                OneOffFeeSaSetuCostsWithbadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsTitleSection5),
                BadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsBadDebtProvisionTitleSection5),
                OneOffFeeSaSetuCostsWithoutbadDebtProvision5 = CurrencyConverterUtils.ConvertToCurrency(data.SaSetupCostsWithBadDebtProvisionTitleSection5),
            };
        }

        private static List<CalcSummaryProducerCalculationResults> ArrangeProducerCalculationResult(
            CalcResult calcResult,
            IEnumerable<int> acceptedProducerIds,
            IImmutableList<MaterialDetail> materials)
        {
            var results = new List<CalcSummaryProducerCalculationResults>();

            var filteredProducers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => acceptedProducerIds.Contains(producer.ProducerIdInt) && !string.IsNullOrWhiteSpace(producer.Level));

            foreach (var producer in filteredProducers)
            {
                results.Add(CalcSummaryProducerCalculationResults.From(producer, materials, calcResult.ApplyModulation));
            }

            return results;
        }

        private static CalcResultProducerCalculationResultsTotal? ArrangeProducerCalculationResultsTotal(CalcResultSummary calcResultSummary)
        {
            return CalcResultProducerCalculationResultsTotal.From(calcResultSummary);
        }
    }
}
