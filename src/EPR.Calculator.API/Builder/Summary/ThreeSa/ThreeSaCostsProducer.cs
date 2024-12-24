using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Summary.Common;
using EPR.Calculator.API.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.API.Builder.Summary.ThreeSa;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.ThreeSA
{
    public static class ThreeSaCostsProducer
    {
        public static readonly int ColumnIndex = 210;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.TotalSaOperatingCostsWoTitleSection3, ColumnIndex = ColumnIndex },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.BadDebtProvisionSection3, ColumnIndex = ColumnIndex + 1 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.SaOperatingCostsWithTitleSection3, ColumnIndex = ColumnIndex + 2 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.EnglandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 3 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.WalesTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 4 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.ScotlandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 5 },
                new CalcResultSummaryHeader { Name = ThreeSaCostHeader.NorthernIrelandTotalWithBadDebtProvisionSection3, ColumnIndex = ColumnIndex + 6 }
            ];
        }


        public static void GetProducerSetUpCostsSection3(CalcResult calcResult, CalcResultSummary summary)
        {
            summary.SaOperatingCostsWoTitleSection3 = ThreeSaCostsSummary.GetThreeSaCostsWithoutBadDebtProvision(calcResult);
            summary.BadDebtProvisionTitleSection3 = (summary.SaOperatingCostsWoTitleSection3 * ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult)) / 100;
            summary.SaOperatingCostsWithTitleSection3 = summary.BadDebtProvisionTitleSection3 + summary.SaOperatingCostsWoTitleSection3;

            foreach (var item in summary.ProducerDisposalFees)
            {
                item.Total3SAOperatingCostwoBadDebtprovision = GetTotalProducerFeeWithoutBadDebtProvisionSection3(summary, item);
                item.BadDebtProvisionFor3 = GetBadDebtProvisionSection3(calcResult, item);
                item.Total3SAOperatingCostswithBadDebtprovision = item.Total3SAOperatingCostwoBadDebtprovision + item.BadDebtProvisionFor3;
                item.EnglandTotalWithBadDebtProvision3 = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult), item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.England);
                item.WalesTotalWithBadDebtProvision3 = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult), item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Wales);
                item.ScotlandTotalWithBadDebtProvision3 = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult), item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.Scotland);
                item.NorthernIrelandTotalWithBadDebtProvision3 = GetCountryTotalWithBadDebtProvision(calcResult, summary.SaOperatingCostsWoTitleSection3, ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult), item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries.NorthernIreland);
            }
        }

        private static decimal GetBadDebtProvisionSection3(CalcResult calcResult, CalcResultSummaryProducerDisposalFees item)
        {
            return (item.Total3SAOperatingCostwoBadDebtprovision * ThreeSaCostsSummary.GetSetUpBadDebtProvision(calcResult)) / 100;
        }

        private static decimal GetTotalProducerFeeWithoutBadDebtProvisionSection3(CalcResultSummary summary, CalcResultSummaryProducerDisposalFees item)
        {
            return (item.ProducerOverallPercentageOfCostsForOnePlus2A2B2C * summary.SaOperatingCostsWoTitleSection3) / 100;
        }

        public static decimal GetCountryTotalWithBadDebtProvision(CalcResult calcResult, decimal threeSaCostsWithoutBadDebtProvision, decimal badDebtProvisionSection3, decimal ProducerOverallPercentageOfCostsForOnePlus2A2B2C, Countries country)
        {
            var countryTotal = (CalcResultSummaryUtil.GetCountryOnePlusFourApportionment(calcResult, country)) / 100;
            return threeSaCostsWithoutBadDebtProvision * (1 + (badDebtProvisionSection3 / 100)) * (ProducerOverallPercentageOfCostsForOnePlus2A2B2C / 100) * countryTotal;
        }

    }
}