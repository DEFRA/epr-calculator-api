using EnumsNET;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class AvailableClassificationsService(ApplicationDBContext context, ILogger<AvailableClassificationsService> logger) : IAvailableClassificationsService
{
    public async Task<List<CalculatorRunClassification>> GetAvailableClassificationsForFinancialYearAsync(CalcFinancialYearRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            List<RunClassification> validStatuses = await this.DetermineAvailableClassificationsAsync(request, cancellationToken);
            if (validStatuses.Count == 0)
            {
                return new();
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
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred whilst attempting to determine available classifications.");
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

    private async Task<List<RunClassification>> DetermineAvailableClassificationsAsync(CalcFinancialYearRequestDto request, CancellationToken cancellationToken)
    {
        List<RunClassificationStatus> currentClassifications = await this.GetCurrentClassifications(request, cancellationToken);

        if (IsPreInitialRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INITIAL_RUN,
                RunClassification.TEST_RUN,
            };
        }

        if (IsDesignatedButIncomplete(currentClassifications))
        {
            return new()
            {
                RunClassification.TEST_RUN,
            };
        }

        if (HasNeitherFinalRunNorFinalRecalculationRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN,
            };
        }

        if (HasFinalRecalculationButNoFinalRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.FINAL_RUN,
                RunClassification.TEST_RUN,
            };
        }

        if (HasFinalRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN,
                RunClassification.TEST_RUN,
            };
        }

        return new();
    }

    private async Task<List<RunClassificationStatus>> GetCurrentClassifications(CalcFinancialYearRequestDto request, CancellationToken cancellationToken)
    {
        List<CalculatorRun> currentRuns = await context.CalculatorRuns
            .Where(run => run.FinancialYearId == request.FinancialYear && run.Id != request.RunId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return currentRuns
            .Select(run => (RunClassification)run.CalculatorRunClassificationId)
            .Select(classification => (RunClassificationStatus)Enum.Parse(typeof(RunClassificationStatus), classification.ToString()))
            .ToList();
    }
}
