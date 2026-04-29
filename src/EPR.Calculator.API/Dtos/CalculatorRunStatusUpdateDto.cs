using EPR.Calculator.API.Data.Enums;

namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunStatusUpdateDto
    {
        public required int RunId { get; init; }

        public required RunClassification Classification { get; init; }
    }
}
