using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ILapcapDataValidator
    {
        ValidationResultDto<CreateLapcapDataErrorDto> Validate(CreateLapcapDataDto createLapcapDataDto);
    }
}