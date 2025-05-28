using System.ComponentModel;

namespace EPR.Calculator.API.Dtos
{
    public record ProducersInstructionResponse
    {
        public ProducersInstructionSummary ProducersInstructionSummary { get; set; }

        public List<ProducersInstructionDetail> ProducersInstructionDetails { get; set; }
    }

    public class ProducersInstructionSummary
    {
        public Dictionary<BillingStatus, int> Statuses { get; set; }
    }

    public class ProducersInstructionDetail
    {
        public int organisationId { get; set; }

        public string organisationName { get; set; }

        public string billingInstruction { get; set; }

        public string invoiceAmount { get; set; }

        public BillingStatus status { get; set; }
    }

    public enum BillingStatus
    {
        Noaction = 0,
        Accepted = 1,
        Rejected = 2,
        Pending = 3,
    }
}
