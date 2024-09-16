using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICreateDefaultParameterDataValidator
    {
        ValidationResultDto Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto);
    }
}