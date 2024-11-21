using static Azure.Core.HttpHeader;

namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public CalcResultSummaryHeader ResultSummaryHeader { get; set; }

        public IEnumerable<CalcResultSummaryHeader> ProducerDisposalFeesHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader> MaterialBreakdownHeaders { get; set; }

        public IEnumerable<CalcResultSummaryHeader> ColumnHeaders { get; set; }

        //Section-7
        public decimal TotalFeeforLADisposalCostswoBadDebtprovision1 { get; set; }

        public decimal BadDebtProvisionFor1 { get; set; }
        
        public decimal TotalFeeforLADisposalCostswithBadDebtprovision1 { get; set; }
        
        public decimal TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A { get; set; }
        
        public decimal BadDebtProvisionFor2A { get; set; }
        
        public decimal TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A { get; set; }

        //Section-4
        public decimal LaDataPrepCostsTitleSection4 { get; set; }

        public decimal LaDataPrepCostsBadDebtProvisionTitleSection4 { get; set; }

        public decimal LaDataPrepCostsWithBadDebtProvisionTitleSection4 { get; set; }

        public IEnumerable<CalcResultSummaryProducerDisposalFees> ProducerDisposalFees { get; set; }
    }
}
