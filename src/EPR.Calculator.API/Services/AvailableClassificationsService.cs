using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IAvailableClassificationsService
{
    Task<List<CalculatorRunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default);
}

public class AvailableClassificationsService(
    ApplicationDBContext context
) : IAvailableClassificationsService
{
    public async Task<List<CalculatorRunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default)
    {
        var validStatuses = await DetermineAvailableClassificationsAsync(request, cancellationToken);

        if (validStatuses.Count == 0)
            return [];

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

    private static bool IsPreInitialRun(List<RunClassificationStatus> currentClassifications) => currentClassifications.TrueForAll(c => !c.HasFlag(RunClassificationStatus.Designated));

    private static bool IsDesignatedButIncomplete(List<RunClassificationStatus> currentClassifications) => currentClassifications.Exists(c => c.HasFlag(RunClassificationStatus.Designated) && !c.HasFlag(RunClassificationStatus.Complete));

    private static bool HasNeitherFinalRunNorFinalRecalculationRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Exists(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
               && currentClassifications.TrueForAll(c => c != RunClassificationStatus.FINAL_RECALCULATION_RUN_COMPLETED && c != RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private static bool HasFinalRecalculationButNoFinalRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Exists(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
               && currentClassifications.Exists(c => c == RunClassificationStatus.FINAL_RECALCULATION_RUN_COMPLETED)
               && currentClassifications.TrueForAll(c => c != RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private static bool HasFinalRun(List<RunClassificationStatus> currentClassifications)
    {
        return currentClassifications.Exists(c => c == RunClassificationStatus.INITIAL_RUN_COMPLETED)
               && currentClassifications.Exists(c => c == RunClassificationStatus.FINAL_RUN_COMPLETED);
    }

    private async Task<bool> IsCurrentRunOlderThanOtherCompletedRuns(
        CalculatorRun currentRun,
        List<CalculatorRun> filteredRuns,
        CancellationToken cancellationToken)
    {
        var compledRunIds = filteredRuns.Where(run => run.CalculatorRunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED
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
                    calculatorRunBillingFileMetadata.BillingFileAuthorisedDate
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return runs.Where(run => run.BillingFileAuthorisedDate.HasValue)
            .Any(run => run.BillingFileAuthorisedDate!.Value >= currentRun.CreatedAt);
    }

    private async Task<List<RunClassification>> DetermineAvailableClassificationsAsync(
        CalcRelativeYearRequestDto request,
        CancellationToken cancellationToken)
    {
        var allRuns = await GetCalculatorRuns(request, cancellationToken);
        List<CalculatorRun> filteredRuns = [.. allRuns.Where(run => run.Id != request.RunId)];
        List<RunClassificationStatus> currentClassifications = [.. filteredRuns.Select(run => (RunClassification)run.CalculatorRunClassificationId).Select(classification => (RunClassificationStatus)Enum.Parse(typeof(RunClassificationStatus), classification.ToString()))];

        if (IsPreInitialRun(currentClassifications))
        {
            return
            [
                RunClassification.INITIAL_RUN,
                RunClassification.TEST_RUN
            ];
        }

        if (IsDesignatedButIncomplete(currentClassifications))
        {
            return
            [
                RunClassification.TEST_RUN
            ];
        }

        var currentRun = allRuns.Single(run => run.Id == request.RunId);

        if (await IsCurrentRunOlderThanOtherCompletedRuns(currentRun, filteredRuns, cancellationToken))
        {
            return
            [
                RunClassification.TEST_RUN
            ];
        }

        if (HasNeitherFinalRunNorFinalRecalculationRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN
            ];
        }

        if (HasFinalRecalculationButNoFinalRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN
            ];
        }

        if (HasFinalRun(currentClassifications))
        {
            return
            [
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.TEST_RUN
            ];
        }

        return [];
    }

    private async Task<List<CalculatorRun>> GetCalculatorRuns(CalcRelativeYearRequestDto request, CancellationToken cancellationToken)
    {
        var currentRuns = await context.CalculatorRuns
            .Where(run => run.RelativeYear == request.RelativeYearValue
                          && run.CalculatorRunClassificationId != (int)RunClassification.DELETED
                          && run.CalculatorRunClassificationId != (int)RunClassification.ERROR
                          && run.CalculatorRunClassificationId != (int)RunClassification.RUNNING
                          && run.CalculatorRunClassificationId != (int)RunClassification.INTHEQUEUE)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return currentRuns;
    }
}
