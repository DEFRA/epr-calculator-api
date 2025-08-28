namespace EPR.Calculator.API.Dtos
{
    public class FinancialYearClassificationResponseDto
    {
        public string FinancialYear { get; set; }

        public List<CalculatorRunClassificationDto> Classifications { get; set; }

        public List<ClassifiedCalculatorRunDto>? ClassifiedRuns { get; set; }
    }
}
