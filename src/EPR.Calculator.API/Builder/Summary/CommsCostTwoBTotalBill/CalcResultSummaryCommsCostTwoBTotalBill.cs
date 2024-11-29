using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.CommsCostTwoBTotalBill
{
    public static class CalcResultSummaryCommsCostTwoBTotalBill
    {
        #region Constants
        private const string England = "England";
        private const string Wales = "Wales";
        private const string Scotland = "Scotland";
        private const string NorthernIreland = "NorthernIreland";
        #endregion

        #region TotalsRow
        public static decimal GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal producerFeeWithoutBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithoutBadDebt += GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, allResults);
            }

            return producerFeeWithoutBadDebt;
        }

        public static decimal GetCommsBadDebtProvisionFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal BadDebtProvision = 0;

            foreach (var producer in producers)
            {
                BadDebtProvision += GetCommsBadDebtProvisionFor2b(calcResult, producer, allResults);
            }

            return BadDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal producerFeeWithBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithBadDebt += GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, allResults);
            }

            return producerFeeWithBadDebt;
        }

        public static decimal GetCommsEnglandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal englandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                englandWithBadDebt += GetCommsEnglandWithBadDebt(calcResult, producer, allResults);
            }

            return englandWithBadDebt;
        }

        public static decimal GetCommsWalesWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal walesWithBadDebt = 0;

            foreach (var producer in producers)
            {
                walesWithBadDebt += GetCommsWalesWithBadDebt(calcResult, producer, allResults);
            }

            return walesWithBadDebt;
        }

        public static decimal GetCommsScotlandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal scotlandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                scotlandWithBadDebt += GetCommsScotlandWithBadDebt(calcResult, producer, allResults);
            }

            return scotlandWithBadDebt;
        }

        public static decimal GetCommsNorthernIrelandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                northernIrelandWithBadDebt += GetCommsNorthernIrelandWithBadDebt(calcResult, producer, allResults);
            }

            return northernIrelandWithBadDebt;
        }
        #endregion

        #region Single RowbyRow
        public static decimal GetCommsEnglandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt(calcResult, producer, allResults, England);
        }

        public static decimal GetCommsWalesWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt(calcResult, producer, allResults, Wales);
        }

        public static decimal GetCommsScotlandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt(calcResult, producer, allResults, Scotland);
        }

        public static decimal GetCommsNorthernIrelandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt(calcResult, producer, allResults, NorthernIreland);
        }

        public static decimal GetCommsWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults, string region)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults) / 100;
            decimal regionApportionment = GetRegionApportionment(calcResult, region);
            decimal badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%')) / 100;
            return commsCostHeaderWithoutBadDebtFor2bTitle * (1 + badDebtProvision) * percentageOfProducerReportedHHTonnagevsAllProducers * regionApportionment;
        }

        public static decimal GetRegionApportionment(CalcResult calcResult, string region)
        {
            var apportionmentDetails = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails;

            return region switch
            {
                England => Convert.ToDecimal(apportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')) / 100,
                Wales => Convert.ToDecimal(apportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')) / 100,
                Scotland => Convert.ToDecimal(apportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')) / 100,
                NorthernIreland => Convert.ToDecimal(apportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')) / 100,
                _ => throw new ArgumentException("Invalid region specified")
            };
        }

        public static decimal GetCommsBadDebtProvisionFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal producerFeeWithoutBadDebtFor2b =GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, allResults);
            decimal badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%')) / 100;
            return producerFeeWithoutBadDebtFor2b * badDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return CalculateProducerFee(calcResult, producer, allResults, includeBadDebt: true);
        }

        public static decimal CalculateProducerFee(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults, bool includeBadDebt)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults) / 100;
            decimal badDebtProvisionFor2b = GetCommsBadDebtProvisionFor2b(calcResult, producer, allResults);
            decimal producerFeeWithoutBadDebt = commsCostHeaderWithoutBadDebtFor2bTitle * percentageOfProducerReportedHHTonnagevsAllProducers;

            if (!includeBadDebt)
                return producerFeeWithoutBadDebt;

            return producerFeeWithoutBadDebt + badDebtProvisionFor2b;
        }

        public static decimal GetCommsProducerFeeWithoutBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return CalculateProducerFee(calcResult, producer, allResults, includeBadDebt: false);
        }
        #endregion
    }
}
