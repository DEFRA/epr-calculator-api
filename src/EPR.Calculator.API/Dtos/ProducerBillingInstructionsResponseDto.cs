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
    /// Gets or sets the total number of records by status.
    /// </summary>
    public int TotalAcceptedRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of records by status.
    /// </summary>
    public int TotalRejectedRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of records by status.
    /// </summary>
    public int TotalPendingRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of Initial billing instruction records.
    /// </summary>
    public int TotalInitialRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of Delta billing instruction records.
    /// </summary>
    public int TotalDeltaRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of Rebill billing instruction records.
    /// </summary>
    public int TotalRebillRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of Cancel Bill billing instruction records.
    /// </summary>
    public int TotalCancelBillRecords { get; set; }

    /// <summary>
    /// Gets or sets the total number of No Action billing instruction records.
    /// </summary>
    public int TotalNoActionRecords { get; set; }

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

    public IEnumerable<int> AllProducerIds { get; set; } = new List<int>();
}