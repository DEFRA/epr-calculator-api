using System.Linq.Expressions;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers;

public static class CalcRunMapper
{
    // tech debt: SpecifyKind should be at the EF level
    public static readonly Expression<Func<CalculatorRun, CalculatorRunDto>> ToDto = run =>
        new CalculatorRunDto
        {
            RunId = run.Id,
            RelativeYear = run.RelativeYear,
            RunName = run.Name,
            RunClassification = run.Classification,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy,
            UpdatedAt = run.UpdatedAt,
            UpdatedBy = run.UpdatedBy,
            BillingRunStatus = run.BillingRunStatus,
            BillingRunStartedAt = run.BillingRunStartedAt,
            BillingFile = run.CalculatorRunBillingFileMetadata
                .OrderByDescending(m => m.BillingFileCreatedDate)
                .Select(m => new CalculatorRunDto.BillingFileDto
                {
                    Id = m.Id,
                    IsLatest = run.ProducerResultFileSuggestedBillingInstruction
                        .Where(x => x.LastModifiedAcceptReject != null)
                        .All(x => x.LastModifiedAcceptReject <= m.BillingFileCreatedDate),
                    CsvFileName = m.BillingCsvFileName,
                    JsonFileName = m.BillingJsonFileName,
                    CreatedAt = m.BillingFileCreatedDate,
                    CreatedBy = m.BillingFileCreatedBy,
                    HasBeenSentToFss = RunClassificationHelper.CompletedClassifications.Contains(run.Classification),
                    SentAt = m.BillingFileAuthorisedDate,
                    SentBy = m.BillingFileAuthorisedBy
                })
                .FirstOrDefault()
        };
}
