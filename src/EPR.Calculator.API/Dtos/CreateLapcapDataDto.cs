using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataDto
    {
        public required string ParameterYear { get; set; }
        public required IEnumerable<LapcapDataTemplateValueDto> LapcapDataTemplateValues { get; set; }
    }
}
