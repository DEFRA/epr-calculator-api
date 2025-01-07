using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Validators
{
    public interface IRpdStatusDataValidator
    {
        RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId, IEnumerable<CalculatorRunClassification> calculatorRunClassifications);
        RpdStatusValidation IsValidSuccessfulRun(int runId);
    }
}
