using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Models
{
    public record CalculatorRunMessage: MessageBase
    {
        public required RelativeYear RelativeYear { get; set; }

        public required string CreatedBy { get; set; }
    }
}