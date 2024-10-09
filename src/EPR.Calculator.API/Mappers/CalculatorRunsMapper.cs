using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public class CalculatorRunMapper
    {
        public static CalculatorRunDetailsDto? Map(CalculatorRun calculatorRun)
        {
            if (calculatorRun == null)
            {
                return null;
            }
            else
            {
                var result = new CalculatorRunDetailsDto
                {
                    Id = calculatorRun.Id,
                    CalculatorRunName = calculatorRun.Name,
                    FinancialYear = calculatorRun.Financial_Year,
                    CreatedAt = calculatorRun.CreatedAt,
                    RunClassificationId = calculatorRun.CalculatorRunClassificationId,
                    CreatedBy = calculatorRun.CreatedBy,
                };
                return result;
            }
        }
    }
}