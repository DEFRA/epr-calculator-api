using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataDto
    {
        public string ParameterYear { get; set; }
        public IEnumerable<LapcapDataTemplateValueDto> LapcapDataTemplateValues { get; set; }
    }
}
