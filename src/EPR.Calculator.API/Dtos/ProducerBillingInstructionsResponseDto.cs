namespace EPR.Calculator.API.Dtos;

/// <summary>
/// Represents a response for producer billing instructions for a calculator run.
/// </summary>
public class ProducerBillingInstructionsResponseDto
{
    public List<ProducerBillingInstructionsDto> Records { get; set; } = new List<ProducerBillingInstructionsDto>();

    /// <summary>
    /// Gets or sets the total number of records.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the calculator run Id.
    /// </summary>
    public int CalculatorRunId { get; set; }

    /// <summary>
    /// Gets or sets the calculator run name.
    /// </summary>
    public string? RunName { get; set; }

    /// <summary>
    /// Gets or sets the page number for pagination.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size for pagination.
    /// </summary>
    public int? PageSize { get; set; }
}