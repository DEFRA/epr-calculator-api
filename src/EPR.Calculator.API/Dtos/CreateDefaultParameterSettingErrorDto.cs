namespace EPR.Calculator.API.Dtos
{
    public class CreateDefaultParameterSettingErrorDto : ErrorDto
    {
        public string ParameterUniqueRef { get; set; }
        public string ParameterCategory { get; set; }

        public string ParameterType { get; set; }
    }
}