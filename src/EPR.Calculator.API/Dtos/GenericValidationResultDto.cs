namespace EPR.Calculator.API.Dtos
{
    public class GenericValidationResultDto
    {
        public GenericValidationResultDto()
        {
            this.Errors = new List<string>();
        }

        public List<string> Errors { get; set; }

        public bool IsInvalid { get; set; }
    }
}
