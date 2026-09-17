using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Dtos
{
    public class CalculatorRunClassificationDto
    {
        public RunClassification Id { get; set; }

        public string Status { get; set; } = null!;
    }
}
