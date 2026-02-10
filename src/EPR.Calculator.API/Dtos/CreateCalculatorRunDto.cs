using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateCalculatorRunDto
    {
        public required string CalculatorRunName { get; set; }

        public required RelativeYear RelativeYear { get; set; }
    }
}
