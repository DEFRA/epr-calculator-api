namespace EPR.Calculator.API.Dtos;

public class ProducersInstructionDetail
{
    public int OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    public string? BillingInstruction { get; set; }

    public string? InvoiceAmount { get; set; }

    public string? Status { get; set; }
}