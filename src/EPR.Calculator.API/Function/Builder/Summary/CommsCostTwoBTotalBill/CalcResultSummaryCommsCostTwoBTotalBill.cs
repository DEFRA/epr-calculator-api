using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill
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
        public static decimal GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithoutBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithoutBadDebt += GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return producerFeeWithoutBadDebt;
        }

        public static decimal GetCommsBadDebtProvisionFor2bTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal BadDebtProvision = 0;

            foreach (var producer in producers)
            {
                BadDebtProvision += GetCommsBadDebtProvisionFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return BadDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2bTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithBadDebt = 0;

            foreach (var producer in producers)
            {
                producerFeeWithBadDebt += GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            }

            return producerFeeWithBadDebt;
        }

        public static decimal GetCommsEnglandWithBadDebtTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal englandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                englandWithBadDebt += GetCommsEnglandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return englandWithBadDebt;
        }

        public static decimal GetCommsWalesWithBadDebtTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal walesWithBadDebt = 0;

            foreach (var producer in producers)
            {
                walesWithBadDebt += GetCommsWalesWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return walesWithBadDebt;
        }

        public static decimal GetCommsScotlandWithBadDebtTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal scotlandWithBadDebt = 0;

            foreach (var producer in producers)
            {
                scotlandWithBadDebt += GetCommsScotlandWithBadDebt(calcResult, producer, hhTotalPackagingTonnage);
            }

            return scotlandWithBadDebt;
        }

        public static decimal GetCommsNorthernIrelandWithBadDebtTotalsRow(CalcResult calcResult, IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
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
        public static decimal GetCommsEnglandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, England);
        }

        public static decimal GetCommsWalesWithBadDebt(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, Wales);
        }

        public static decimal GetCommsScotlandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, Scotland);
        }

        public static decimal GetCommsNorthernIrelandWithBadDebt(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return GetCommsWithBadDebt(calcResult, producer, hhTotalPackagingTonnage, NorthernIreland);
        }

        public static decimal GetCommsWithBadDebt(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage, string region)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;
            decimal regionApportionment = GetRegionApportionment(calcResult, region) / 100;
            decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
            return commsCostHeaderWithoutBadDebtFor2bTitle * (1 + badDebtProvision) * percentageOfProducerReportedHHTonnagevsAllProducers * regionApportionment;
        }

        public static decimal GetRegionApportionment(CalcResult calcResult, string region)
        {
            var apportionmentDetail = calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment;

            return region switch
            {
                England         => apportionmentDetail.England,
                Wales           => apportionmentDetail.Wales,
                Scotland        => apportionmentDetail.Scotland,
                NorthernIreland => apportionmentDetail.NorthernIreland,
                _               => throw new ArgumentException("Invalid region specified")
            };
        }

        public static decimal GetCommsBadDebtProvisionFor2b(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            decimal producerFeeWithoutBadDebtFor2b = GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, hhTotalPackagingTonnage);
            decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
            return producerFeeWithoutBadDebtFor2b * badDebtProvision;
        }

        public static decimal GetCommsProducerFeeWithBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return CalculateProducerFee(calcResult, producer, hhTotalPackagingTonnage, includeBadDebt: true);
        }

        public static decimal CalculateProducerFee(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage, bool includeBadDebt)
        {
            decimal commsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, hhTotalPackagingTonnage) / 100;

            decimal producerFeeWithoutBadDebt = commsCostHeaderWithoutBadDebtFor2bTitle * percentageOfProducerReportedHHTonnagevsAllProducers;

            if (!includeBadDebt)
                return producerFeeWithoutBadDebt;

            decimal badDebtProvisionFor2b = GetCommsBadDebtProvisionFor2b(calcResult, producer, hhTotalPackagingTonnage);
            return producerFeeWithoutBadDebt + badDebtProvisionFor2b;
        }

        public static decimal GetCommsProducerFeeWithoutBadDebtFor2b(CalcResult calcResult, ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> hhTotalPackagingTonnage)
        {
            return CalculateProducerFee(calcResult, producer, hhTotalPackagingTonnage, includeBadDebt: false);
        }

        #endregion
    }
}
