namespace EPR.Calculator.API.Dtos
{
    public class ValidationResultDto
    {
        public ValidationResultDto() { this.Errors = new List<ErrorDto>(); }

        public List<ErrorDto> Errors { get; set; }

        public bool IsInvalid { get; set; }
    }
}
