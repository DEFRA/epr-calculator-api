using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunsParamsDto
    {
        public required RelativeYear RelativeYear { get; set; }
    }
}
