using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.TwoCCommsCost
{
    public class TwoCCommsCostUtil
    {
        public static void UpdateTwoCTotals(CalcResult calcResult, IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees, bool isOverAllTotalRow,
            CalcResultSummaryProducerDisposalFees totalRow)
        {
            if (isOverAllTotalRow)
            {
                var level1Rows = producerDisposalFees
                    .Where(pf => pf.Level == ((int)CalcResultSummaryLevelIndex.One).ToString()).ToList();
                var englandTotal = level1Rows.Sum(x => x.TwoCEnglandTotalWithBadDebt);
                var walesTotal = level1Rows.Sum(x => x.TwoCWalesTotalWithBadDebt);
                var scotlandTotal = level1Rows.Sum(x => x.TwoCScotlandTotalWithBadDebt);
                var niTotal = level1Rows.Sum(x => x.TwoCNorthernIrelandTotalWithBadDebt);
                totalRow.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt =
                    level1Rows.Sum(x => x.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt);

                totalRow.TwoCBadDebtProvision =  level1Rows.Sum(x => x.TwoCBadDebtProvision);

                totalRow.TwoCEnglandTotalWithBadDebt = englandTotal;

                totalRow.TwoCWalesTotalWithBadDebt = walesTotal;

                totalRow.TwoCScotlandTotalWithBadDebt = scotlandTotal;

                totalRow.TwoCNorthernIrelandTotalWithBadDebt = niTotal;

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

                var level2Rows = producerDisposalFees
                    .Where(pf => pf.Level == ((int)CalcResultSummaryLevelIndex.Two).ToString()).ToList();

                var englandTotal = level2Rows.Sum(x => x.TwoCEnglandTotalWithBadDebt);
                var walesTotal = level2Rows.Sum(x => x.TwoCWalesTotalWithBadDebt);
                var scotlandTotal = level2Rows.Sum(x => x.TwoCScotlandTotalWithBadDebt);
                var niTotal = level2Rows.Sum(x => x.TwoCNorthernIrelandTotalWithBadDebt);

                totalRow.TwoCEnglandTotalWithBadDebt = englandTotal;

                totalRow.TwoCWalesTotalWithBadDebt = walesTotal;

                totalRow.TwoCScotlandTotalWithBadDebt = scotlandTotal;

                totalRow.TwoCNorthernIrelandTotalWithBadDebt = niTotal;
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

            var englandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).EnglandTotal;
            result.TwoCEnglandTotalWithBadDebt =
                englandTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            var walesTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).WalesTotal;
            result.TwoCWalesTotalWithBadDebt =
                walesTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            var scotlandTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).ScotlandTotal;
            result.TwoCScotlandTotalWithBadDebt =
                scotlandTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;

            var niTotal = calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).NorthernIrelandTotal;
            result.TwoCNorthernIrelandTotalWithBadDebt =
                niTotal * result.TwoCTotalProducerFeeForCommsCostsWithBadDebt / 100;
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
    }
}
