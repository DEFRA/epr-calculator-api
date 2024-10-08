using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public class CalculatorRunsMapper
    {
        public static CalculatorRunsDetailsDto Map(CalculatorRun calculatorRuns)
        {
            if (calculatorRuns == null)
            {
                return null;
            }
            else
            {
                var result = new CalculatorRunsDetailsDto
                {
                    Id = calculatorRuns.Id,
                    CalculatorRunName = calculatorRuns.Name,
                    FinancialYear = calculatorRuns.Financial_Year,
                    CreatedAt = calculatorRuns.CreatedAt,
                    RunClassificationId = calculatorRuns.CalculatorRunClassificationId,
                    CreatedBy = calculatorRuns.CreatedBy,
                };
                return result;
            }
        }
    }
}