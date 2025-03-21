using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class SchemeParameterTemplateValueDto
    {
        public required string ParameterUniqueReferenceId { get; set; }

        public required string ParameterValue { get; set; }
    }
}
