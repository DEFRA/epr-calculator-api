using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class LapcapValidationResultDto
    {
        public LapcapValidationResultDto() { this.Errors = new List<CreateLapcapDataErrorDto>(); }

        public List<CreateLapcapDataErrorDto> Errors { get; set; }

        public bool IsInvalid { get; set; }
    }
}
