using EPR.Calculator.API.Dtos;

namespace api.Validators
{
    public interface ICreateDefaultParameterDataValidator
    {
        ValidationResultDto Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto);
    }
}