using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class CalcRunBillingFileMapper
    {
        public static readonly string FileExtension = "CSV";

        public static CalculatorRunBillingDto Map(CalculatorRun run, CalculatorRunClassification classification, CalculatorRunBillingFileMetadata? calculatorRunBillingFileMetadata = null)
        {
            return new CalculatorRunBillingDto
            {
                RunId = run.Id,
                CreatedAt = run.CreatedAt,
                FileExtension = FileExtension,
                RunName = run.Name,
                UpdatedBy = run.UpdatedBy,
                CreatedBy = run.CreatedBy,
                UpdatedAt = run.UpdatedAt,
                RunClassificationId = run.CalculatorRunClassificationId,
                RunClassificationStatus = classification.Status,
                FinancialYear = run.FinancialYearId,
                BillingFileId = calculatorRunBillingFileMetadata?.Id,
                BillingCsvFileName = calculatorRunBillingFileMetadata?.BillingCsvFileName,
                BillingJsonFileName = calculatorRunBillingFileMetadata?.BillingJsonFileName,
                BillingFileCreatedBy = calculatorRunBillingFileMetadata?.BillingFileCreatedBy,
                BillingFileAuthorisedBy = calculatorRunBillingFileMetadata?.BillingFileAuthorisedBy,
                BillingFileCreatedDate = calculatorRunBillingFileMetadata?.BillingFileCreatedDate,
                BillingFileAuthorisedDate = calculatorRunBillingFileMetadata?.BillingFileAuthorisedDate,
            };
        }
    }
}
