namespace EPR.Calculator.API.Data.DataModels
{
    public class ProducerDesignatedRunInvoiceInstruction
    {
        public int Id { get; set; }

        public int ProducerId { get; set; }

        public int CalculatorRunId { get; set; }

        public decimal? CurrentYearInvoicedTotalAfterThisRun { get; set; }

        public decimal? InvoiceAmount { get; set; }

        public decimal? OutstandingBalance { get; set; }

        public string? BillingInstructionId { get; set; }

        public DateTime? InstructionConfirmedDate { get; set; }

        public string? InstructionConfirmedBy { get; set; }
    }
}
