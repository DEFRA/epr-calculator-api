namespace EPR.Calculator.API.Dtos
{
    public class CreateDefaultParameterSettingDto
    {
        public string ParameterYear { get; set; }
        public IEnumerable<SchemeParameterTemplateValueDto> SchemeParameterTemplateValues { get; set; }
    }
}
