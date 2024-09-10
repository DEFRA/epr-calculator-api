using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ErrorDto
    {
        public required string Message { get; set; }

        public string? Description { get; set; }
    }
}
