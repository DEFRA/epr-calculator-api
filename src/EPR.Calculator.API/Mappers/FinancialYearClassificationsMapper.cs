using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class FinancialYearClassificationsMapper
    {
        public static FinancialYearClassificationResponseDto Map(
            string financialYear,
            IEnumerable<CalculatorRunClassification> classifications)
        {
            return new FinancialYearClassificationResponseDto
            {
                FinancialYear = financialYear,
                Classifications = classifications.Select(c =>
                    new CalculatorRunClassificationDto
                    {
                        Id = c.Id,
                        Status = c.Status,
                    }).ToList(),
            };
        }

        public static FinancialYearClassificationResponseDto Map(
            string financialYear,
            IEnumerable<CalculatorRunClassification> classifications,
            List<ClassifiedCalculatorRunDto> runs)
        {
            if (runs is null)
            {
                return Map(financialYear, classifications);
            }

            return new FinancialYearClassificationResponseDto
            {
                FinancialYear = financialYear,
                Classifications = classifications.Select(c =>
                    new CalculatorRunClassificationDto
                    {
                        Id = c.Id,
                        Status = c.Status,
                    }).ToList(),
                ClassifiedRuns = runs,
            };
        }
    }
}
