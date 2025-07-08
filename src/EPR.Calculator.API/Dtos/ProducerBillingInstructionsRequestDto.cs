using EPR.Calculator.API.Constants;

namespace EPR.Calculator.API.Dtos
{
    /// <summary>
    /// Represents a request to request producer billing instructions for a calculator run.
    /// </summary>
    public class ProducerBillingInstructionsRequestDto
    {
        /// <summary>
        /// Gets or sets the search term for filtering.
        /// </summary>
        public ProducerBillingInstructionsSearchQueryDto? SearchQuery { get; set; }

        /// <summary>
        /// Gets or sets the page number for pagination.
        /// </summary>
        public int? PageNumber { get; set; } = CommonConstants.ProducerBillingInstructionsDefaultPageNumber;

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int? PageSize { get; set; } = CommonConstants.ProducerBillingInstructionsDefaultPageSize;
    }
}
