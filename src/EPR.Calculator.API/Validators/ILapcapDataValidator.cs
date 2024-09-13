using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ILapcapDataValidator
    {
        LapcapValidationResultDto Validate(CreateLapcapDataDto createLapcapDataDto);
    }
}