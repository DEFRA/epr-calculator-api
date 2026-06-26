using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Dtos
{
    public class RelativeYearClassificationResponseDto
    {
        public required RelativeYear RelativeYear { get; set; }

        public List<CalculatorRunClassificationDto> Classifications { get; set; } = [];

        public List<CalculatorRunDto> ClassifiedRuns { get; set; } = [];
    }
}
