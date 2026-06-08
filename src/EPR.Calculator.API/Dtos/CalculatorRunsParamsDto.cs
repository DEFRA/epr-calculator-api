using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunsParamsDto
    {
        public required RelativeYear RelativeYear { get; set; }
    }
}
