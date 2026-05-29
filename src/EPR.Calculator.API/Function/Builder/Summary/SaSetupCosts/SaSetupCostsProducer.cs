using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts
{
    public static class SaSetupCostsProducer
    {
        public static ImmutableList<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithoutBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ProducerOneOffFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.EnglandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.WalesTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.ScotlandTotalWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = SaSetupCostsHeaders.NorthernIrelandTotalWithBadDebtProvision }
            ];
        }
        public static ImmutableList<CalcResultSummaryHeader> GetSummaryHeaders(int columnIndex)
        {
            return [
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithoutBadDebtProvisionTitle}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.BadDebtProvisionTitle}", ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = $"{SaSetupCostsHeaders.OneOffFeeSetupCostsWithBadDebtProvisionTitle}", ColumnIndex = columnIndex + 2 }
            ];
        }

        public static void GetProducerSetUpCosts(CalcResult calcResult, CalcResultSummary summary)
        {
            summary.SaSetupCostsTitleSection5 = calcResult.CalcResultParameterOtherCost.SchemeSetupCost.Total;
            summary.SaSetupCostsBadDebtProvisionTitleSection5 = (summary.SaSetupCostsTitleSection5 * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;
            summary.SaSetupCostsWithBadDebtProvisionTitleSection5 = summary.SaSetupCostsBadDebtProvisionTitleSection5 + summary.SaSetupCostsTitleSection5;

            foreach (var item in summary.ProducerDisposalFees)
            {
                var totalProducerFeeWithoutBadDebtProvision = (item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * summary.SaSetupCostsTitleSection5) / 100;
                var badDebtProvision = (totalProducerFeeWithoutBadDebtProvision * calcResult.CalcResultParameterOtherCost.BadDebtValue) / 100;

                item.OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                {
                    TotalProducerFeeWithoutBadDebtProvision  = totalProducerFeeWithoutBadDebtProvision,
                    BadDebtProvision                         = badDebtProvision,
                    TotalProducerFeeWithBadDebtProvision     = totalProducerFeeWithoutBadDebtProvision + badDebtProvision,
                    EnglandTotalWithBadDebtProvision         = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England),
                    WalesTotalWithBadDebtProvision           = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales),
                    ScotlandTotalWithBadDebtProvision        = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland),
                    NorthernIrelandTotalWithBadDebtProvision = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaSetupCostsTitleSection5, item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland)
                };
            }
        }

        public static decimal GetCountryTotalWithBadDebtProvision(
            CalcResult calcResult,
            decimal oneOffFeeSetupCostsWithoutBadDebtProvision,
            decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C,
            Countries country
        )
        {
            return oneOffFeeSetupCostsWithoutBadDebtProvision
               * (1 + (calcResult.CalcResultParameterOtherCost.BadDebtValue / 100))
               * (ProducerOverallPercentageOfCostsForOnePlus2A2B2C / 100)
               * (CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country) / 100);
        }
    }
}
