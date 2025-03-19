using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class FinancialYearMapper
    {
        public static FinancialYearDto Map(CalculatorRunFinancialYear financialYear)
            => new FinancialYearDto
            {
                Name = financialYear.Name,
                Description = financialYear.Description,
            };
    }
}
