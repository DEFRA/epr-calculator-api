using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class AvailableClassificationsService(ApplicationDBContext context)
    : IAvailableClassificationsService
{
    public async Task<List<RunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default)
    {
        var targetRun = await context.CalculatorRuns
            .SingleAsync(run => run.Id == request.RunId, cancellationToken);

        var otherRunsForYear = await context.CalculatorRuns
            .Where(run =>
                run.Id != request.RunId &&
                run.RelativeYearValue == request.RelativeYearValue &&
                run.Classification != RunClassification.Deleted &&
                run.Classification != RunClassification.Errored &&
                run.Classification != RunClassification.Running &&
                run.Classification != RunClassification.None
            )
            .Select(cr => new RunInfo
            {
                RunId = cr.Id,
                RunClassification = cr.Classification,
                BillingRunStatus = cr.BillingRunStatus,
                BillingRunAuthorisedAt = cr.BillingFileMetadata != null
                    ? cr.BillingFileMetadata.BillingFileAuthorisedDate
                    : null
            })
            .ToListAsync(cancellationToken);

        if (HasAllUndesignated(otherRunsForYear))
        {
            return
            [
                RunClassification.InitialRun,
                RunClassification.TestRun
            ];
        }

        if (HasAnyDesignatedButIncomplete(otherRunsForYear))
        {
            return
            [
                RunClassification.TestRun
            ];
        }

        if (HasAnyCompletedAfter(otherRunsForYear, targetRun.CreatedAt))
        {
            return
            [
                RunClassification.TestRun
            ];
        }

        if (HasNeitherFinalRunNorFinalRecalculationRun(otherRunsForYear))
        {
            return
            [
                RunClassification.InterimRecalculationRun,
                RunClassification.FinalRecalculationRun,
                RunClassification.FinalRun,
                RunClassification.TestRun
            ];
        }

        if (HasFinalRecalculationButNoFinalRun(otherRunsForYear))
        {
            return
            [
                RunClassification.InterimRecalculationRun,
                RunClassification.FinalRun,
                RunClassification.TestRun
            ];
        }

        if (HasFinalRun(otherRunsForYear))
        {
            return
            [
                RunClassification.InterimRecalculationRun,
                RunClassification.TestRun
            ];
        }

        return [];
    }

    private static bool HasAllUndesignated(IReadOnlyCollection<RunInfo> runs)
    {
        return runs.All(r => r.RunClassification is not (
            RunClassification.InitialRun or
            RunClassification.InitialRunCompleted or
            RunClassification.InterimRecalculationRun or
            RunClassification.InterimRecalculationRunCompleted or
            RunClassification.FinalRecalculationRun or
            RunClassification.FinalRecalculationRunCompleted or
            RunClassification.FinalRun or
            RunClassification.FinalRunCompleted
            ));
    }

    private static bool HasAnyDesignatedButIncomplete(IReadOnlyCollection<RunInfo> runs)
    {
        return runs.Any(r => r.RunClassification is
            RunClassification.InitialRun or
            RunClassification.InterimRecalculationRun or
            RunClassification.FinalRecalculationRun or
            RunClassification.FinalRun);
    }

    private static bool HasAnyCompletedAfter(IReadOnlyCollection<RunInfo> runs, DateTime cutOff)
    {
        return runs
            .Any(r => r.BillingRunStatus == BillingRunStatus.Completed &&
                      r.BillingRunAuthorisedAt >= cutOff &&
                      r.RunClassification is
                          RunClassification.InitialRunCompleted or
                          RunClassification.InterimRecalculationRunCompleted or
                          RunClassification.FinalRecalculationRunCompleted or
                          RunClassification.FinalRunCompleted);
    }

    private static bool HasNeitherFinalRunNorFinalRecalculationRun(IReadOnlyCollection<RunInfo> runs)
    {
        return runs.Any(r => r.RunClassification is RunClassification.InitialRunCompleted)
               && runs.All(r => r.RunClassification is not (
                   RunClassification.FinalRecalculationRunCompleted or
                   RunClassification.FinalRunCompleted
                   ));
    }

    private static bool HasFinalRecalculationButNoFinalRun(IReadOnlyCollection<RunInfo> runs)
    {
        return runs.Any(r => r.RunClassification is RunClassification.InitialRunCompleted)
               && runs.Any(r => r.RunClassification is RunClassification.FinalRecalculationRunCompleted)
               && runs.All(r => r.RunClassification is not RunClassification.FinalRunCompleted);
    }

    private static bool HasFinalRun(IReadOnlyCollection<RunInfo> runs)
    {
        return runs.Any(r => r.RunClassification is RunClassification.InitialRunCompleted)
               && runs.Any(r => r.RunClassification is RunClassification.FinalRunCompleted);
    }

    private sealed record RunInfo
    {
        public required int RunId { get; init; }
        public required RunClassification RunClassification { get; init; }
        public required BillingRunStatus BillingRunStatus { get; init; }
        public required DateTime? BillingRunAuthorisedAt { get; init; }
    }
}
