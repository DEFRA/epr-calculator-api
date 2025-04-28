using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalculatorRunStatusDataValidator
    {
        ValidationResultDto Validate(CalculatorRunStatusUpdateDto calculatorRunStatusUpdateDto);
    }
}
