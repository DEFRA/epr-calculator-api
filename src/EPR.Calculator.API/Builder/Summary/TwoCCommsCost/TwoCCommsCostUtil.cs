using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;
using System;

namespace EPR.Calculator.API.Builder.Summary.TwoCCommsCost
{
    public class TwoCCommsCostUtil
    {
        #region Constants
        private const string England = "England";
        private const string Wales = "Wales";
        private const string Scotland = "Scotland";
        private const string NorthernIreland = "NorthernIreland";
        #endregion

        public static void UpdateTwoCTotals(CalcResult calcResult,
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees, bool isOverAllTotalRow,
            CalcResultSummaryProducerDisposalFees totalRow, List<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails)
        {
            if (isOverAllTotalRow)
            {
                var level1Rows = producerDisposalFees
                    .Where(pf => pf.Level == ((int)CalcResultSummaryLevelIndex.One).ToString()).ToList();
                totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                    level1Rows.Sum(x => x.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt);
                totalRow.TwoCBadDebtProvision = level1Rows.Sum(x => x.TwoCBadDebtProvision);
            }
            else
            {
                totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                    (totalRow.PercentageofProducerReportedHHTonnagevsAllProducers * calcResult
                        .CalcResultCommsCostReportDetail.CommsCostByCountry.Last().TotalValue) / 100;

                var badDebtProvisionValue = (calcResult.CalcResultParameterOtherCost.BadDebtValue *
                                             calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last()
                                                 .TotalValue) / 100;

                totalRow.TwoCBadDebtProvision = (totalRow.PercentageofProducerReportedHHTonnagevsAllProducers *
                                                 badDebtProvisionValue) / 100;

            }

            totalRow.TwoCTotalProducerFeeForCommsCostsWithBadDebt =
                totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt + totalRow.TwoCBadDebtProvision;

            totalRow.TwoCEnglandTotalWithBadDebt =
                GetCommsEnglandWithBadDebtTotalsRow2C(calcResult, producersAndSubsidiaries,
                    runProducerMaterialDetails);
            totalRow.TwoCWalesTotalWithBadDebt =
                GetCommsWalesWithBadDebtTotalsRow2C(calcResult, producersAndSubsidiaries,
                    runProducerMaterialDetails);
            totalRow.TwoCNorthernIrelandTotalWithBadDebt =
                GetCommsNIWithBadDebtTotalsRow2C(calcResult, producersAndSubsidiaries,
                    runProducerMaterialDetails);
            totalRow.TwoCScotlandTotalWithBadDebt =
                GetCommsScotlandWithBadDebtTotalsRow2C(calcResult, producersAndSubsidiaries,
                    runProducerMaterialDetails);
        }

        public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result,
            ProducerDetail producer, List<CalcResultsProducerAndReportMaterialDetail> runProducerMaterialDetails)
        {
            result.TwoCEnglandTotalWithBadDebt =
                GetCommEnglandWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCWalesTotalWithBadDebt =
                GetCommWalesWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCNorthernIrelandTotalWithBadDebt =
                GetCommNIWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCScotlandTotalWithBadDebt =
                GetCommScotlandWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);

            result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().TotalValue *
                result.PercentageofProducerReportedHHTonnagevsAllProducers / 100;

            var badDebtProvisionValue = (calcResult.CalcResultParameterOtherCost.BadDebtValue *
                                         calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last()
                                             .TotalValue) / 100;
            result.TwoCBadDebtProvision = badDebtProvisionValue *
                result.PercentageofProducerReportedHHTonnagevsAllProducers / 100;

            result.TwoCTotalProducerFeeForCommsCostsWithBadDebt =
                result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt + result.TwoCBadDebtProvision;
        }

        public static void UpdateHeaderTotal(CalcResult calcResult, CalcResultSummary result)
        {
            //Section 2c
            result.TwoCCommsCostsByCountryWithoutBadDebtProvision = calcResult.CalcResultCommsCostReportDetail
                .CommsCostByCountry.Last().TotalValue;

            result.TwoCBadDebtProvision = (calcResult.CalcResultParameterOtherCost.BadDebtValue *
                                           result.TwoCCommsCostsByCountryWithoutBadDebtProvision) / 100;

            result.TwoCCommsCostsByCountryWithBadDebtProvision =
                result.TwoCCommsCostsByCountryWithoutBadDebtProvision + result.TwoCBadDebtProvision;
        }

        public static decimal GetCommsEnglandWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommEnglandWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsWalesWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommWalesWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsNIWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommNIWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsScotlandWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommScotlandWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommWalesWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().WalesValue;
            return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, allResults);
        }

        public static decimal GetCommNIWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().NorthernIrelandValue;
            return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, allResults);
        }

        public static decimal GetCommScotlandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().ScotlandValue;
            return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, allResults);
        }

        public static decimal GetCommEnglandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal twoCCommCostsByCountryBadDebtProvision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().EnglandValue;
            return twoCCommCostsByCountryBadDebtProvision * GetCommWithBadDebt2C(calcResult, producer, allResults);
        }

        public static decimal GetCommWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
            decimal percentageOfProducerReportedHhTonnageVsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults) / 100;
            return (1 + badDebtProvision) * percentageOfProducerReportedHhTonnageVsAllProducers;
        }
    }
}