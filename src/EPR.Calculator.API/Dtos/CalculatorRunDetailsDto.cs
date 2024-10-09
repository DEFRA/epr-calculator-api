namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunDetailsDto
    {
        public int Id { get; set; }

        public int RunClassificationId { get; set; }

        public string CalculatorRunName { get; set; }

        public string FinancialYear { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
