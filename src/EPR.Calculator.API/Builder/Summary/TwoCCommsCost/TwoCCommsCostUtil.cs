using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.HHTonnageVsAllProducer;
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

        public static void UpdateTwoCTotals(CalcResult calcResult, IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees, bool isOverAllTotalRow,
            CalcResultSummaryProducerDisposalFees totalRow)
        {
            if (isOverAllTotalRow)
            {
                var level1Rows = producerDisposalFees
                    .Where(pf => pf.Level == ((int)CalcResultSummaryLevelIndex.One).ToString()).ToList();
                totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                    level1Rows.Sum(x => x.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt);
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
        }

        public static void UpdateTwoCRows(CalcResult calcResult, CalcResultSummaryProducerDisposalFees result)
        {
            result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().TotalValue *
                result.PercentageofProducerReportedHHTonnagevsAllProducers / 100;

            
            var badDebtProvisionValue = (calcResult.CalcResultParameterOtherCost.BadDebtValue *
                                         calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last().TotalValue) / 100;
            result.TwoCBadDebtProvision = badDebtProvisionValue *
                result.PercentageofProducerReportedHHTonnagevsAllProducers / 100;

            result.TwoCTotalProducerFeeForCommsCostsWithBadDebt =
                result.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt + result.TwoCBadDebtProvision;

            //var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
            //    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;
            //result.TwoCEnglandTotalWithBadDebt =
            //    englandTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            //var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
            //    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;
            //result.TwoCWalesTotalWithBadDebt =
            //    walesTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            //var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
            //    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;
            //result.TwoCScotlandTotalWithBadDebt =
            //    scotlandTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            //var niTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
            //    .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;
            //result.TwoCNorthernIrelandTotalWithBadDebt =
            //    niTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;
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
