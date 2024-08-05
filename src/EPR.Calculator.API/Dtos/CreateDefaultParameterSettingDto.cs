using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingDto
    {
        public string ParameterYear { get; set; }
        public IEnumerable<SchemeParameterTemplateValueDto> SchemeParameterTemplateValues { get; set; }
    }
}
