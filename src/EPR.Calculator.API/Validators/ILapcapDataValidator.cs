using EPR.Calculator.API.Dtos;

namespace api.Validators
{
    public interface ILapcapDataValidator
    {
        LapcapValidationResultDto Validate(CreateLapcapDataDto createLapcapDataDto);
    }
}