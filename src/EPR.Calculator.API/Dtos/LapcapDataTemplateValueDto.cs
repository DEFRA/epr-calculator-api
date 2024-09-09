using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataTemplateValueDto
    {
        public string UniqueReference { get; set; }
        public string TotalCost { get; set; }
    }
}
