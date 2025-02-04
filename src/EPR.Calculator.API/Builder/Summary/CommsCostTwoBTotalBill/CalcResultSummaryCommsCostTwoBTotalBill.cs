using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.TonnageVsAllProducer;
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
        public static decimal GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithoutBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithoutBadDebt += GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return producerFeeWithoutBadDebt;
        }

        public static decimal GetCommsBadDebtProvisionFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal BadDebtProvision = 0;

            foreach (var producer in producers)
            {
                BadDebtProvision += GetCommsBadDebtProvisionFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return BadDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2bTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithBadDebt += GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return producerFeeWithBadDebt;
        }

        public static decimal GetCommsEnglandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal englandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                englandWithBadDebt += GetCommsEnglandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return englandWithBadDebt;
        }

        public static decimal GetCommsWalesWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal walesWithBadDebt = 0;

            foreach (var producer in producers)
            {
                walesWithBadDebt += GetCommsWalesWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return walesWithBadDebt;
        }

        public static decimal GetCommsScotlandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal scotlandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                scotlandWithBadDebt += GetCommsScotlandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return scotlandWithBadDebt;
        }

        public static decimal GetCommsNorthernIrelandWithBadDebtTotalsRow(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal northernIrelandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                northernIrelandWithBadDebt += GetCommsNorthernIrelandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return northernIrelandWithBadDebt;
        }
        #endregion

        #region Single RowbyRow
        public static decimal GetCommsEnglandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, England);
        }

        public static decimal GetCommsWalesWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, Wales);
        }

        public static decimal GetCommsScotlandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, Scotland);
        }

        public static decimal GetCommsNorthernIrelandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, NorthernIreland);
        }

        public static decimal GetCommsWithBadDebt(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage, string region)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
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

        public static decimal GetCommsBadDebtProvisionFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithoutBadDebtFor2b =GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            decimal badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%')) / 100;
            return producerFeeWithoutBadDebtFor2b * badDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return CalculateProducerFee(calcResult, producer, hhTotalPackagingTonnage, includeBadDebt: true);
        }

        public static decimal CalculateProducerFee(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage, bool includeBadDebt)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
            
            decimal producerFeeWithoutBadDebt = commsCostHeaderWithoutBadDebtFor2bTitle * percentageOfProducerReportedHHTonnagevsAllProducers;

            if (!includeBadDebt)
                return producerFeeWithoutBadDebt;

            decimal badDebtProvisionFor2b = GetCommsBadDebtProvisionFor2b(calcResult, producer, hhTotalPackagingTonnage);
            return producerFeeWithoutBadDebt + badDebtProvisionFor2b;
        }

        public static decimal GetCommsProducerFeeWithoutBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IEnumerable<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return CalculateProducerFee(calcResult, producer, hhTotalPackagingTonnage, includeBadDebt: false);
        }

        #endregion
    }
}
