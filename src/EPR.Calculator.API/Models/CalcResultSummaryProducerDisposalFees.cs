namespace EPR.Calculator.API.Models
{
    public class CalcResultSummaryProducerDisposalFees
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public int Level { get; set; }

        public required Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> ProducerDisposalFeesByMaterial { get; set; }

        public Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> CommsCostByMaterial { get; set; }
    }
}
