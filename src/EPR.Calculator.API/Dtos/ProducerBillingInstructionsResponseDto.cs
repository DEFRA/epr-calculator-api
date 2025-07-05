using System.Net;

namespace EPR.Calculator.API.Dtos;

/// <summary>
/// Represents a response for producer billing instructions for a calculator run.
/// </summary>
public class ProducerBillingInstructionsResponseDto
{
    /// <summary>
    /// Gets or sets the status code of the service process.
    /// </summary>
    /// <value>
    /// The HTTP status code representing the result of the service process.
    /// </value>
    public HttpStatusCode StatusCode { get; set; }

    public List<ProducerBillingInstructionsDto> Records { get; set; } = new List<ProducerBillingInstructionsDto>();

    /// <summary>
    /// Gets or sets the total number of records.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Returns the page number for pagination.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Returns the page size for pagination.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Returns the calculator run name.
    /// </summary>
    public string RunName { get; set; }
}