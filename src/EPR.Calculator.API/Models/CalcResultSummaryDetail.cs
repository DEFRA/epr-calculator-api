namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryDetail
    {
        public int ProducerId { get; set; }
        public string SubsidaryId { get; set; }
        public string SubsidaryName { get; set; }
        public decimal ReportedHouseholdPackagingWasteTonnage { get; set; }
        public decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }
        public decimal NetReportedTonnage { get; set; }
        public decimal PricePerTonne { get; set; }
    }
}
