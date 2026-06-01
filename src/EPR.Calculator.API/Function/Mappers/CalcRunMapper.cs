using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mappers
{
    public static class CalcRunMapper
    {
        private static readonly string FileExtension = "CSV";

        public static CalculatorRunDto Map(CalculatorRun run, CalculatorRunClassification classification)
        {
            return new CalculatorRunDto
            {
                RunId = run.Id,
                CreatedAt = run.CreatedAt,
                FileExtension = FileExtension,
                RunName = run.Name ?? string.Empty,
                UpdatedBy = run.UpdatedBy,
                UpdatedAt = run.UpdatedAt,
                RunClassificationId = run.CalculatorRunClassificationId,
                RunClassificationStatus = classification.Status,
            };
        }
    }
}
