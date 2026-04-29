using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalculatorRunStatusDataValidator
    {
        GenericValidationResultDto Validate(
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto);

        GenericValidationResultDto Validate(
            List<CalculatorRunDto> designatedRuns,
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto);
    }
}
