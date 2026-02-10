using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Models
{
    public class CalculatorRunMessage
    {
        public required int CalculatorRunId { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public required string CreatedBy { get; set; }

        public required string MessageType { get; set; }
    }
}