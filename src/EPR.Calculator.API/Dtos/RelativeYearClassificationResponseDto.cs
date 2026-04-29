using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    public class RelativeYearClassificationResponseDto
    {
        public required RelativeYear RelativeYear { get; set; }

        public List<RunClassification> Classifications { get; set; } = [];

        public List<CalculatorRunDto> ClassifiedRuns { get; set; } = [];
    }
}
