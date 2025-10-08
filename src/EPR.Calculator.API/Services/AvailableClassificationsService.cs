using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class AvailableClassificationsService(
    ApplicationDBContext context,
    ILogger<AvailableClassificationsService> logger) : IAvailableClassificationsService
{
    public async Task<List<CalculatorRunClassification>> GetAvailableClassificationsForFinancialYearAsync(CalcFinancialYearRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            List<RunClassification> validStatuses = await this.DetermineAvailableClassificationsAsync(request, cancellationToken);

            if (validStatuses.Count == 0)
            {
                return [];
            }

            var allClassifications = await context.CalculatorRunClassifications
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return validStatuses
                .Join(
                    allClassifications,
                    status => (int)status,
                    classification => classification.Id,
                    (status, classification) => classification)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred whilst attempting to determine available classifications. Error :-{Message}", ex.Message);
            throw;
        }
    }

    private static bool IsPreInitialRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.All(c => !c.HasFlag(RunClassificationStatus.Designated));
    }

    private static bool IsDesignatedButIncomplete(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Any(c => c.HasFlag(RunClassificationStatus.Designated) && !c.HasFlag(RunClassificationStatus.Complete));
    }

    private static bool HasNeitherFinalRunNorFinalRecalculationRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Any(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
            && currentClassifications.All(c => c != RunClassificationStatus.FINAL_RECALCULATION_RUN_COMPLETED && c != RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private static bool HasFinalRecalculationButNoFinalRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Any(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
            && currentClassifications.Any(c => c == RunClassificationStatus.FINAL_RECALCULATION_RUN_COMPLETED)
            && currentClassifications.All(c => c != RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private static bool HasFinalRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Any(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
            && currentClassifications.Any(c => c == RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private async Task<bool> IsCurrentRunOlderThanOtherCompletedRuns(
       CalculatorRun currentRun,
       List<CalculatorRun> filteredRuns,
       CancellationToken cancellationToken)
    {
        IList<int> compledRunIds = filteredRuns.Where(run => run.CalculatorRunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED
                                                   || run.CalculatorRunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                                                   || run.CalculatorRunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED
                                                   || run.CalculatorRunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED)
                                               .Select(run => run.Id)
                                               .ToList();

        var runs = await
                (from run in context.CalculatorRuns
                 join calculatorRunBillingFileMetadata in context.CalculatorRunBillingFileMetadata
                            on run.Id equals calculatorRunBillingFileMetadata.CalculatorRunId
                 where compledRunIds.Contains(run.Id)
                 select new
                 {
                     RunId = run.Id,
                     calculatorRunBillingFileMetadata.BillingFileAuthorisedDate,
                 })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        return runs.Where(run => run.BillingFileAuthorisedDate.HasValue)
                   .All(run => (run.BillingFileAuthorisedDate!.Value >= currentRun.CreatedAt));
    }

    private async Task<List<RunClassification>> DetermineAvailableClassificationsAsync(
        CalcFinancialYearRequestDto request,
        CancellationToken cancellationToken)
    {
        List<CalculatorRun> allRuns = await this.GetCalculatorRuns(request, cancellationToken);
        List<CalculatorRun> filteredRuns = [.. allRuns.Where(run => run.Id != request.RunId)];
        List<RunClassificationStatus> currentClassifications = [.. filteredRuns.Select(run => (RunClassification)run.CalculatorRunClassificationId).Select(classification => (RunClassificationStatus)Enum.Parse(typeof(RunClassificationStatus), classification.ToString()))];

        if (IsPreInitialRun(currentClassifications))
        {
            return
            [
                RunClassification.INITIAL_RUN,
                RunClassification.TEST_RUN,
            ];
        }

        if (IsDesignatedButIncomplete(currentClassifications))
        {
            return
            [
                RunClassification.TEST_RUN,
            ];
        }

        CalculatorRun currentRun = allRuns.Single(run => run.Id == request.RunId);

        if (await IsCurrentRunOlderThanOtherCompletedRuns(currentRun, filteredRuns, cancellationToken))
        {
            return
            [
                RunClassification.TEST_RUN,
            ];
        }

        if (HasNeitherFinalRunNorFinalRecalculationRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN,
            ];
        }

        if (HasFinalRecalculationButNoFinalRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN,
            ];
        }

        if (HasFinalRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.TEST_RUN,
            ];
        }

        return [];
    }

    private async Task<List<CalculatorRun>> GetCalculatorRuns(CalcFinancialYearRequestDto request, CancellationToken cancellationToken)
    {
        List<CalculatorRun> currentRuns = await context.CalculatorRuns
            .Where(run => run.FinancialYearId == request.FinancialYear 
                && (run.CalculatorRunClassificationId != (int)RunClassification.DELETED
                || run.CalculatorRunClassificationId != (int)RunClassification.ERROR
                || run.CalculatorRunClassificationId != (int)RunClassification.RUNNING
                || run.CalculatorRunClassificationId != (int)RunClassification.INTHEQUEUE))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return currentRuns;
    }
}
