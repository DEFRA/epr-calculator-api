using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalculatorRunStatusDataValidator
    {
        GenericValidationResultDto Validate(CalculatorRun calculatorRun,
            CalculatorRunClassification classification,
            CalculatorRunStatusUpdateDto runStatusUpdateDto);
    }
}
