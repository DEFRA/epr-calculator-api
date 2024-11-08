namespace EPR.Calculator.API.Models
{
    public class CalcResultSummary
    {
        public int ProducerId { get; set; }

        public string SubsidiaryId { get; set; }

        public string ProducerName { get; set; }

        public Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryMaterialCost>> MaterialCostSummary { get; set; }
    }
}
