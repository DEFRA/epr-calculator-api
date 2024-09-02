using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Commands
{
    public class CreateDefaultParameterCommand
    {
        public bool IsInvalid { get; set; }

        public IEnumerable<CreateDefaultParameterSettingErrorDto> ValidationErrors { get; set; }
        public string ParameterYear { get; set; }
        public IEnumerable<SchemeParameterTemplateValueDto> SchemeParameterTemplateValues { get; set; }
    }
}
