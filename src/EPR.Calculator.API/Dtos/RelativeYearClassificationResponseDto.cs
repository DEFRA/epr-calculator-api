using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    public class RelativeYearClassificationResponseDto
    {
        public required RelativeYear RelativeYear { get; set; }

        public List<CalculatorRunClassificationDto> Classifications { get; set; } = [];

        public List<ClassifiedCalculatorRunDto> ClassifiedRuns { get; set; } = [];
    }
}
