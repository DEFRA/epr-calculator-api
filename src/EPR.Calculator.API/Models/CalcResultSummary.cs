namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public string ProducerId { get; set; }

        public string SubsidiaryId { get; set; }

        public string ProducerName { get; set; }

        public Dictionary<int, IEnumerable<CalcResultSummaryMaterialCost>> MaterialCostSummary { get; set; }
    }
}
