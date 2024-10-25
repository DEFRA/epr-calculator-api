using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Validators
{
    public interface IRpdStatusDataValidator
    {
        public RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId, IEnumerable<CalculatorRunClassification> calculatorRunClassifications);
        public RpdStatusValidation IsValidSuccessfulRun(int runId);
    }
}
