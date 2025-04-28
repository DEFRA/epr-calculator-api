using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalculatorRunStatusDataValidator
    {
        GenericValidationResultDto Validate(CalculatorRunStatusUpdateDto calculatorRunStatusUpdateDto);
    }
}
