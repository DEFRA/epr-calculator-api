namespace EPR.Calculator.API.Dtos
{
    public class ValidationResultDto
    {
        public ValidationResultDto() { this.Errors = new List<CreateDefaultParameterSettingErrorDto>(); }

        public List<CreateDefaultParameterSettingErrorDto> Errors { get; set; }

        public bool IsInvalid { get; set; }
    }
}
