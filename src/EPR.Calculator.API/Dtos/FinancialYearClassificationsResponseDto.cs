namespace EPR.Calculator.API.Dtos
{
    public class FinancialYearClassificationResponseDto
    {
        public string FinancialYear { get; set; } = null!;

        public List<CalculatorRunClassificationDto> Classifications { get; set; } = null!;
    }
}
