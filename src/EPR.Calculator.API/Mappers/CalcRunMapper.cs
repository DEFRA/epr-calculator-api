using System.Linq.Expressions;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Mappers;

public static class CalcRunMapper
{
    public static readonly Expression<Func<CalculatorRun, CalculatorRunDto>> ToDto = run =>
        new CalculatorRunDto
        {
            RunId = run.Id,
            RelativeYear = run.RelativeYear,
            RunName = run.Name,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy,
            UpdatedAt = run.UpdatedAt,
            UpdatedBy = run.UpdatedBy,
            RunClassification = (RunClassification) run.CalculatorRunClassificationId,
            BillingRunStatus = run.BillingRunStatus,
            BillingRunStartedAt = run.BillingRunStartedAt,
            CompletedBillingRun = run.BillingFileMetadata != null
                ? new CalculatorRunDto.BillingMetadataDto
                {
                    CsvFileName = run.BillingFileMetadata.BillingCsvFileName,
                    JsonFileName = run.BillingFileMetadata.BillingJsonFileName,
                    CreatedAt = run.BillingFileMetadata.BillingFileCreatedDate,
                    CreatedBy = run.BillingFileMetadata.BillingFileCreatedBy,
                    AuthorisedAt = run.BillingFileMetadata.BillingFileAuthorisedDate,
                    AuthorisedBy = run.BillingFileMetadata.BillingFileAuthorisedBy
                }
                : null
        };
}
