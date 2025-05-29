using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Dtos
{
    /// <summary>
    /// Represents a request to Produce billing instruction.
    /// </summary>
    public class ProduceBillingInstuctionRequestDto
    {
        public required List<int> OrganisationIds { get; set; }

        public required string Status { get; set; }

        public string? ReasonForRejection { get; set; }
    }
}
