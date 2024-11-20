namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryHeader ResultSummaryHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader> ProducerDisposalFeesHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader> ColumnHeaders { get; set; }

        public decimal TwoCCommsCostsByCountryWithoutBadDebtProvision {get; set; }

        public decimal TwoCBadDebtProvision { get; set; }

        public decimal TwoCCommsCostsByCountryWithBadDebtProvision { get; set; }

        public decimal LaDataPrepCostsTitleSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionTitleSection4 { get; set; }

        public decimal LaDataPrepCostsWithBadDebtProvisionTitleSection4 { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
    }
}
