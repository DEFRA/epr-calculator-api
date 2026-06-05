using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Misc
{
    public class CalcResultsRequestDto
    {
        public required int RunId { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public ImmutableHashSet<int> AcceptedProducerIds { get; set; } = [];

        public bool IsBillingFile { get; set; }

        public string ApprovedBy { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
    }
}
