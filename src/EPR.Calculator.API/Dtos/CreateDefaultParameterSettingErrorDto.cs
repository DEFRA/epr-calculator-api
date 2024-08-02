namespace EPR.Calculator.API.Dtos
{
    public class CreateDefaultParameterSettingErrorDto : ErrorDto
    {
        public string ParameterCategory { get; set; }

        public string ParameterType { get; set; }
    }
}