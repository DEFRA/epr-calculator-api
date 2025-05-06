using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.Dtos
{
    [ExcludeFromCodeCoverage]
    public class ValidationResultDto<TErrorDto>
        where TErrorDto : ErrorDto
    {
        public ValidationResultDto()
        {
            this.Errors = new List<TErrorDto>();
        }

        public List<TErrorDto> Errors { get; set; }

        public bool IsInvalid { get; set; }
    }
}
