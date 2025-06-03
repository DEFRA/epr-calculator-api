using System.ComponentModel;

namespace EPR.Calculator.API.Dtos
{
    public record ProducersInstructionResponse
    {
        public ProducersInstructionSummary? ProducersInstructionSummary { get; set; }

        public List<ProducersInstructionDetail>? ProducersInstructionDetails { get; set; }
    }

    public class ProducersInstructionSummary
    {
        public Dictionary<string, int>? Statuses { get; set; }
    }

    public class ProducersInstructionDetail
    {
        public int OrganisationId { get; set; }

        public string? OrganisationName { get; set; }

        public string? BillingInstruction { get; set; }

        public string? InvoiceAmount { get; set; }

        public string? Status { get; set; }
    }
}
