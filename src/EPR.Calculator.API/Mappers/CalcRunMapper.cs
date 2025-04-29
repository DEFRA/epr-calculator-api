using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class CalcRunMapper
    {
        public static readonly string FileExtension = "CSV";

        public static CalculatorRunDto Map(CalculatorRun run, CalculatorRunClassification classification)
        {
            return new CalculatorRunDto
            {
                RunId = run.Id,
                CreatedAt = run.CreatedAt,
                FileExtension = FileExtension,
                RunName = run.Name ?? string.Empty,
                UpdatedBy = run.UpdatedBy ?? string.Empty,
                CreatedBy = run.CreatedBy,
                UpdatedAt = run.UpdatedAt,
                RunClassificationId = run.CalculatorRunClassificationId,
                RunClassificationStatus = classification.Status,
                FinancialYear = run.Financial_Year.Name,
            };
        }
    }
}
