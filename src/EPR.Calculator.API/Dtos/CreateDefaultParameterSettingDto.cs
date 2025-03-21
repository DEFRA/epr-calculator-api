using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingDto
    {
        public required string ParameterYear { get; set; }

        public required IEnumerable<SchemeParameterTemplateValueDto> SchemeParameterTemplateValues { get; set; }

        public required string ParameterFileName { get; set; }
    }
}
