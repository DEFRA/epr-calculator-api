﻿using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

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
                GetCommsEnglandWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCWalesTotalWithBadDebt =
                GetCommsWalesWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCNorthernIrelandTotalWithBadDebt =
                GetCommsNIWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);
            result.TwoCScotlandTotalWithBadDebt =
                GetCommsScotlandWithBadDebt2C(calcResult, producer, runProducerMaterialDetails);

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
                northernIrelandTotalwithBadDebtprovision += GetCommsEnglandWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsWalesWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommsWalesWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsNIWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommsNIWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsScotlandWithBadDebtTotalsRow2C(CalcResult calcResult, IEnumerable<ProducerDetail> producers, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var producer in producers)
            {
                northernIrelandTotalwithBadDebtprovision += GetCommsScotlandWithBadDebt2C(calcResult, producer, allResults);
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetCommsWalesWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt2C(calcResult, producer, allResults, Wales);
        }

        public static decimal GetCommsNIWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt2C(calcResult, producer, allResults, NorthernIreland);
        }

        public static decimal GetCommsScotlandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt2C(calcResult, producer, allResults, Scotland);
        }

        public static decimal GetCommsEnglandWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults)
        {
            return GetCommsWithBadDebt2C(calcResult, producer, allResults, England);
        }

        public static decimal GetCommsWithBadDebt2C(CalcResult calcResult, ProducerDetail producer, IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults, string region)
        {
            decimal twocCommsCostsbyCountryBadDebtprovision = calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().TotalValue;
            decimal badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue /100;
            decimal percentageOfProducerReportedHHTonnagevsAllProducers = HHTonnageVsAllProducerUtil.GetPercentageofProducerReportedHHTonnagevsAllProducers(producer, allResults) / 100;
            decimal regionApportionment = GetRegionApportionment2C(calcResult, region);
            return twocCommsCostsbyCountryBadDebtprovision * (1 + badDebtProvision) * percentageOfProducerReportedHHTonnagevsAllProducers * regionApportionment;
        }

        public static decimal GetRegionApportionment2C(CalcResult calcResult, string region)
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
    }
}
