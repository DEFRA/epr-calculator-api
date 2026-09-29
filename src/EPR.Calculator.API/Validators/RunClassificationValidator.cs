using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Queries;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Validators;

public interface IRunClassificationValidator
{
    Task<GenericValidationResultDto> ValidateAsync(CalculatorRun runToValidate, RunClassification newClassification, CancellationToken cancellationToken);
}

public class RunClassificationValidator(ApplicationDBContext dbContext)
    : IRunClassificationValidator
{
    private static readonly ImmutableHashSet<RunClassification> Assignable =
    [
        RunClassification.Test,
        RunClassification.Initial,
        RunClassification.Recalculation,
        RunClassification.Deleted
    ];

    public async Task<GenericValidationResultDto> ValidateAsync(
        CalculatorRun runToValidate,
        RunClassification newClassification,
        CancellationToken cancellationToken)
    {
        if (!Assignable.Contains(newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"The classification '{newClassification}' is not assignable." ]
            };
        }

        if (runToValidate.IsBillingFileShared)
        {
            return new GenericValidationResultDto
            {
                Errors = ["Cannot reclassify a run once the run is completed."]
            };
        }

        if (runToValidate.Classification == RunClassification.Deleted)
        {
            return new GenericValidationResultDto
            {
                Errors = ["Cannot reclassify a run once the run is deleted."]
            };
        }

        // Runs can be deleted regardless of their calculation status, e.g. to discard errored runs
        if (newClassification == RunClassification.Deleted)
            return new GenericValidationResultDto();

        if (runToValidate.CalculationRunStatus != CalculationRunStatus.Completed)
        {
            return new GenericValidationResultDto
            {
                Errors = ["Cannot classify a run until its calculation has completed."]
            };
        }

        var yearInfo = await GetRelativeYearStatus(runToValidate, cancellationToken);

        if (IsDesignatingOfficialButYearHasIncomplete(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"There are outstanding runs for '{runToValidate.RelativeYear}' that have not been completed." ]
            };
        }

        if (IsDesignatingInitialButYearHasCompleted(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"A completed Initial run already exists for '{runToValidate.RelativeYear}'." ]
            };
        }

        if (IsDesignatingRecalculationButYearLacksInitialCompleted(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"Recalculation is not possible as there are no completed Initial runs for '{runToValidate.RelativeYear}'." ]
            };
        }

        if (IsDesignatingOfficialButYearHasNewerFileShared(yearInfo, newClassification, runToValidate.CreatedAt))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"This '{runToValidate.RelativeYear}' run cannot be classified as it is outdated." ]
            };
        }

        return new GenericValidationResultDto();
    }

    private async Task<RelativeYearInfo> GetRelativeYearStatus(CalculatorRun runToValidate, CancellationToken cancellationToken = default)
    {
        return await dbContext.CalculatorRuns
            .WhereOfficialForYear(runToValidate.RelativeYear)
            .Where(run => run.Id != runToValidate.Id)
            .GroupBy(_ => 1)
            .Select(filteredRuns => new RelativeYearInfo
            {
                HasInitialIncomplete       = filteredRuns.Any(run => run.Classification == RunClassification.Initial && !run.IsBillingFileShared),
                HasInitialCompleted        = filteredRuns.Any(run => run.Classification == RunClassification.Initial && run.IsBillingFileShared),
                HasRecalculationIncomplete = filteredRuns.Any(run => run.Classification == RunClassification.Recalculation && !run.IsBillingFileShared),
                HasRecalculationCompleted  = filteredRuns.Any(run => run.Classification == RunClassification.Recalculation && run.IsBillingFileShared),
                LatestFileSharedAt         = filteredRuns.Where(run => run.IsBillingFileShared).Max(run => run.BillingFileSharedAt)
            })
            .SingleOrDefaultAsync(cancellationToken) ?? new RelativeYearInfo();
    }

    private static bool IsDesignatingOfficialButYearHasIncomplete(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is RunClassification.Initial or RunClassification.Recalculation
               && yearInfo is { HasOfficialIncomplete: true };
    }

    private static bool IsDesignatingInitialButYearHasCompleted(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is RunClassification.Initial
               && yearInfo is { HasInitialCompleted: true };
    }

    private static bool IsDesignatingRecalculationButYearLacksInitialCompleted(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is RunClassification.Recalculation
               && yearInfo is { HasInitialCompleted: false };
    }

    private static bool IsDesignatingOfficialButYearHasNewerFileShared(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification,
        DateTime requestedRunCreatedAt)
    {
        return requestedClassification is RunClassification.Initial or RunClassification.Recalculation
               && yearInfo.LatestFileSharedAt >= requestedRunCreatedAt;
    }

    private sealed record RelativeYearInfo
    {
        public bool HasInitialIncomplete { get; init; }
        public bool HasInitialCompleted { get; init; }
        public bool HasRecalculationIncomplete { get; init; }
        public bool HasRecalculationCompleted { get; init; }
        public DateTime? LatestFileSharedAt { get; init; }
        public bool HasOfficialIncomplete => HasInitialIncomplete || HasRecalculationIncomplete;
    }
}
