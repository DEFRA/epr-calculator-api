using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunStatusDataValidator : ICalculatorRunStatusDataValidator
    {
        public CalculatorRunStatusDataValidator()
        {
        }

        public GenericValidationResultDto Validate(
            CalculatorRun calculatorRun,
            CalculatorRunClassification classification,
            CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            throw new NotImplementedException();
            //switch(runStatusUpdateDto.ClassificationId)
            //{
            //    case Calc
            //}
        }
    }
}
