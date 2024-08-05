using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class SchemeParameterTemplateValueDto
    {
        public string ParameterUniqueReferenceId { get; set; }
        public decimal ParameterValue { get; set; }
    }
}
