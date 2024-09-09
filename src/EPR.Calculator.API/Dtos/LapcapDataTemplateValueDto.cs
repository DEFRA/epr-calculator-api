using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataTemplateValueDto
    {
        public required string UniqueReference { get; set; }
        public required string TotalCost { get; set; }
    }
}
