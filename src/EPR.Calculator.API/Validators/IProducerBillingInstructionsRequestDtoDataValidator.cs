using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface IProducerBillingInstructionsRequestDtoDataValidator
    {
        ValidationResultDto<ErrorDto> Validate(ProducerBillingInstructionsRequestDto request);
    }
}