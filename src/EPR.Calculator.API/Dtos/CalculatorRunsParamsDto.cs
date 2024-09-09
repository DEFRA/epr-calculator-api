using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunsParamsDto
    {
        public required string FinancialYear { get; set; }
    }
}
