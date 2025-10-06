using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class CalculationRunService : ICalculationRunService
{
    private readonly ApplicationDBContext context;
    private readonly ILogger<CalculationRunService> logger;
    private readonly IEnumerable<int> wantedClassificationIds;

    public CalculationRunService(ApplicationDBContext context, ILogger<CalculationRunService> logger)
    {
        this.context = context;
        this.logger = logger;
        this.wantedClassificationIds =
        [
            (int)RunClassification.INITIAL_RUN,
            (int)RunClassification.INITIAL_RUN_COMPLETED,
            (int)RunClassification.INTERIM_RECALCULATION_RUN,
            (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
            (int)RunClassification.FINAL_RECALCULATION_RUN,
            (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
            (int)RunClassification.FINAL_RUN,
            (int)RunClassification.FINAL_RUN_COMPLETED,
        ];
    }

    public async Task<List<ClassifiedCalculatorRunDto>> GetDesignatedRunsByFinanialYear(string financialYear, CancellationToken cancellationToken = default)
    {
        try
        {
            var runs = await
                (from run in this.context.CalculatorRuns
                 join classification in this.context.CalculatorRunClassifications
                     on run.CalculatorRunClassificationId equals classification.Id
                 join calculatorRunBillingFileMetadata in this.context.CalculatorRunBillingFileMetadata
                            on run.Id equals calculatorRunBillingFileMetadata.CalculatorRunId into billingFileMetadataGroup
                 from billingFileMetadata in billingFileMetadataGroup.DefaultIfEmpty()
                 where run.FinancialYearId == financialYear && this.wantedClassificationIds.Contains(run.CalculatorRunClassificationId)
                 select new ClassifiedCalculatorRunDto
                 {
                     RunId = run.Id,
                     CreatedAt = run.CreatedAt,
                     RunName = run.Name,
                     RunClassificationId = run.CalculatorRunClassificationId,
                     RunClassificationStatus = classification.Status,
                     UpdatedAt = run.UpdatedAt,
                     BillingFileAuthorisedBy = billingFileMetadata != null ? billingFileMetadata.BillingFileAuthorisedBy : string.Empty,
                     BillingFileAuthorisedDate = billingFileMetadata != null ? billingFileMetadata.BillingFileAuthorisedDate : null,
                 })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return runs;
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "An error occurred whilst attempting to get designated calculator runs for financial year {FinancialYear}.", financialYear);
            throw;
        }
    }
}