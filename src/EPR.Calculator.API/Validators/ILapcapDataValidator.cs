using EPR.Calculator.API.Dtos;

namespace api.Validators
{
    public interface ILapcapDataValidator
    {
        ValidationResultDto Validate(CreateLapcapDataDto createLapcapDataDto);
    }
}