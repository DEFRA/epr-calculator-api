using System.Linq.Expressions;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Mappers;

public static class CalcRunMapper
{
    private static readonly HashSet<int> SentToFssClassifications =
    [
        RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
        RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
        RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
        RunClassificationStatusIds.FINALRUNCOMPLETEDID
    ];

    // tech debt: SpecifyKind should be at the EF level
    public static readonly Expression<Func<CalculatorRun, CalculatorRunDto>> ToDto = run =>
        new CalculatorRunDto
        {
            RunId = run.Id,
            RelativeYear = run.RelativeYear,
            RunName = run.Name,
            RunClassification = (RunClassification) run.CalculatorRunClassificationId,
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
                    CsvFileName = m.BillingCsvFileName!,
                    JsonFileName = m.BillingJsonFileName!,
                    CreatedAt = m.BillingFileCreatedDate,
                    CreatedBy = m.BillingFileCreatedBy,
                    HasBeenSentToFss = SentToFssClassifications.Contains(run.CalculatorRunClassificationId),
                    SentAt = m.BillingFileAuthorisedDate,
                    SentBy = m.BillingFileAuthorisedBy
                })
                .FirstOrDefault()
        };
}
