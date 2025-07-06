namespace EPR.Calculator.API.Dtos;

/// <summary>
/// Represents a data object on the response for producer billing instructions for a calculator run.
/// </summary>
public class ProducerBillingInstructionsDto
{
    public string? ProducerName { get; set; }

    public int ProducerId { get; set; }

    public string SuggestedBillingInstruction { get; set; } = string.Empty;

    public decimal SuggestedInvoiceAmount { get; set; }

    public string BillingInstructionAcceptReject { get; set; } = string.Empty;

    public int CalculatorRunId { get; set; }
}