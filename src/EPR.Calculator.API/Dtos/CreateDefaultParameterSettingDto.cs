using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingDto
    {
        public required RelativeYear RelativeYear { get; set; }

        public required IEnumerable<SchemeParameterTemplateValueDto> SchemeParameterTemplateValues { get; set; }

        public required string ParameterFileName { get; set; }
    }
}
