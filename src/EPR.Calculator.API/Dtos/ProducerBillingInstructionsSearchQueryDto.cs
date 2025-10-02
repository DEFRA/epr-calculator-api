namespace EPR.Calculator.API.Dtos;

/// <summary>
/// Represents search criteria for producer billing instructions.
/// </summary>
public class ProducerBillingInstructionsSearchQueryDto
{
    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public int? OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the status or statuses to filter by.
    /// </summary>
    public IEnumerable<string>? Status { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the billing instruction or billing instruction to filter by.
    /// </summary>
    public IEnumerable<string>? BillingInstruction { get; set; } = new List<string>();
}