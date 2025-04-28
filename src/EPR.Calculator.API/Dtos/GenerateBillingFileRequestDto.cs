namespace EPR.Calculator.API.Dtos
{
    /// <summary>
    /// Represents a request to generate a billing file.
    /// </summary>
    public class GenerateBillingFileRequestDto
    {
        /// <summary>
        /// Gets or sets the identifier for the calculator run.
        /// </summary>
        /// <value>
        /// The unique identifier of the calculator run associated with the billing file generation.
        /// </value>
        public int CalculatorRunId { get; set; }
    }
}
