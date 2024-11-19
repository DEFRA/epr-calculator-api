namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryHeader ResultSummaryHeader { get; set; }

        public CalcResultSummaryHeader ProducerDisposalFeesHeader { get; set; }

        public CalcResultSummaryHeader CommsCostHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeaders { get; set; }

        public IEnumerable<string> ColumnHeaders { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }

        public CalcResultSummaryHeader TotalProducerCostWithDebt { get; set; }

        public CalcResultSummaryHeader TotalProducerPercentageCostCost { get; set; }

        //Rekha
        
        public CalcResultSummaryHeader CommsSLAWoHeader { get; set; }

        public CalcResultSummaryHeader CommsSLAWithHeader { get; set; }
        public CalcResultSummaryHeader TotalProducerFeeforSAOperatingCostsbyMaterialwoBadDebtprovision { get; set; }
        public CalcResultSummaryHeader TotalBadDebtProvisionThree { get; set; }
        public CalcResultSummaryHeader TotalProducerFeeforSAOperatingCostsbyMaterialwithBadDebtprovision { get; set; }

        
    }
}
