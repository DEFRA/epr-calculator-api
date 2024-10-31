namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryDetail
    {
        public int ProducerId { get; set; }
        public string SubsidaryId { get; set; }
        public string SubsidaryName { get; set; }
        public decimal HouseholdPackagingWasteTonnage { get; set; }
        public decimal SelfManagedConsumerWasteTonnage { get; set; }

    }
}
