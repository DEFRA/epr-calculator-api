namespace EPR.Calculator.API.Dtos
{
    public class ProducerBillingInstructionDetails
    {
        public string? ProducerName { get; set; }

        public int ProducerId { get; set; }

        public string SuggestedBillingInstruction { get; set; } = string.Empty;

        public decimal SuggestedInvoiceAmount { get; set; }

        public string? BillingInstructionAcceptReject { get; set; }

        public string? SubsidaryId { get; set; }
    }
}
