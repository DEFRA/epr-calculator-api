namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultRejectedProducer
    {
        public int ProducerId { get; set; }

        public required string ProducerName { get; set; }

        public required string TradingName { get; set; }

        public required string SuggestedBillingInstruction { get; set; }

        public decimal SuggestedInvoiceAmount { get; set; }

        public DateTime? InstructionConfirmedDate { get; set; }

        public required string InstructionConfirmedBy { get; set; }

        public required string ReasonForRejection { get; set; }
        public int runId { get; set; }
    }
}