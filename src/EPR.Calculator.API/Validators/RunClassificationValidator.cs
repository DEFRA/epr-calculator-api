using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
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
    private static readonly Dictionary<RunClassification, RunClassification> ValidTransitions = new()
    {
        [RunClassification.Initial]                 = RunClassification.Unclassified,
        [RunClassification.InitialCompleted]       = RunClassification.Initial,
        [RunClassification.Recalculation]           = RunClassification.Unclassified,
        [RunClassification.RecalculationCompleted] = RunClassification.Recalculation
    };

    private static readonly Dictionary<RunClassification, RunClassification[]> InvalidFromStates = new()
    {
        [RunClassification.Deleted]  = [RunClassification.Deleted],
        [RunClassification.Test] = [RunClassification.Test, RunClassification.Running, RunClassification.Errored, RunClassification.Deleted]
    };

    public async Task<GenericValidationResultDto> ValidateAsync(
        CalculatorRun runToValidate,
        RunClassification newClassification,
        CancellationToken cancellationToken)
    {
        var initialCheck = ValidateClassificationTransition(runToValidate, newClassification);

        if(initialCheck.IsInvalid)
            return initialCheck;

        if(!newClassification.IsOfficial)
            return new GenericValidationResultDto();

        var yearInfo = await GetRelativeYearStatus(runToValidate, cancellationToken);

        if (IsDesignatingIncompleteButAlreadyExists(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"There are outstanding runs for '{runToValidate.RelativeYear}' that have not been completed." ]
            };
        }

        if (IsDesignatingInitialButAlreadyCompleted(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"A completed Initial run already exists for '{runToValidate.RelativeYear}'." ]
            };
        }

        if (IsDesignatingRecalculationWithoutInitial(yearInfo, newClassification))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"Recalculation is not possible as there are no completed Initial runs for '{runToValidate.RelativeYear}'." ]
            };
        }

        if (IsDesignatingOutdatedRun(yearInfo, newClassification, runToValidate.CreatedAt))
        {
            return new GenericValidationResultDto
            {
                Errors = [ $"This '{runToValidate.RelativeYear}' run cannot be classified as it is outdated." ]
            };
        }

        return new GenericValidationResultDto();
    }

    private static GenericValidationResultDto ValidateClassificationTransition(
        CalculatorRun calculatorRun,
        RunClassification requestedClassification)
    {
        if (calculatorRun.Classification.IsCompleted)
        {
            return new GenericValidationResultDto
            {
                Errors = ["Cannot reclassify a run once the run is completed."]
            };
        }

        if (ValidTransitions.TryGetValue(requestedClassification, out var requiredCurrent))
        {
            return calculatorRun.Classification == requiredCurrent
                ? ValidResultDto()
                : InvalidResultDto(requestedClassification);
        }

        if (InvalidFromStates.TryGetValue(requestedClassification, out var invalidStates))
        {
            return invalidStates.Contains(calculatorRun.Classification)
                ? InvalidResultDto(requestedClassification)
                : ValidResultDto();
        }

        return new GenericValidationResultDto
        {
            Errors = ["Invalid Classification"]
        };

        static GenericValidationResultDto ValidResultDto() => new();

        static GenericValidationResultDto InvalidResultDto(RunClassification classification) => new()
        {
            Errors = [string.Format(CommonResources.InvalidClassification, classification)]
        };
    }

    private async Task<RelativeYearInfo> GetRelativeYearStatus(CalculatorRun runToValidate, CancellationToken cancellationToken = default)
    {
        return await dbContext.CalculatorRuns
            .Where(run => run.RelativeYear == runToValidate.RelativeYear
                          && RunClassificationHelper.OfficialClassifications.Contains(run.Classification)
                          && run.Id != runToValidate.Id)
            .GroupBy(_ => 1)
            .Select(filteredRuns => new RelativeYearInfo
            {
                HasInitial                = filteredRuns.Any(run => run.Classification == RunClassification.Initial),
                HasInitialCompleted       = filteredRuns.Any(run => run.Classification == RunClassification.InitialCompleted),
                HasRecalculation          = filteredRuns.Any(run => run.Classification == RunClassification.Recalculation),
                HasRecalculationCompleted = filteredRuns.Any(run => run.Classification == RunClassification.RecalculationCompleted),
                LatestFssFileSentAt       = filteredRuns
                                            .Where(run => run.Classification == RunClassification.InitialCompleted
                                                          || run.Classification == RunClassification.RecalculationCompleted)
                                            .SelectMany(run => run.CalculatorRunBillingFileMetadata)
                                            .Max(m => m.BillingFileAuthorisedDate)
            })
            .SingleOrDefaultAsync(cancellationToken) ?? new RelativeYearInfo();
    }

    private sealed record RelativeYearInfo
    {
        public bool HasInitial { get; init; }
        public bool HasInitialCompleted { get; init; }
        public bool HasRecalculation { get; init; }
        public bool HasRecalculationCompleted { get; init; }
        public DateTime? LatestFssFileSentAt { get; init; }
        public bool HasDesignated => HasInitial || HasInitialCompleted || HasRecalculation || HasRecalculationCompleted;
        public bool HasCompleted => HasInitialCompleted || HasRecalculationCompleted;
    }

    private static bool IsDesignatingIncompleteButAlreadyExists(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is { IsOfficial: true, IsCompleted: false }
               && yearInfo is { HasDesignated: true, HasCompleted: false };
    }

    private static bool IsDesignatingInitialButAlreadyCompleted(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is RunClassification.Initial or RunClassification.InitialCompleted
               && yearInfo is { HasInitialCompleted: true };
    }

    private static bool IsDesignatingRecalculationWithoutInitial(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification)
    {
        return requestedClassification is RunClassification.Recalculation or RunClassification.RecalculationCompleted
               && yearInfo is { HasInitialCompleted: false };
    }

    private static bool IsDesignatingOutdatedRun(
        RelativeYearInfo yearInfo,
        RunClassification requestedClassification,
        DateTime requestedRunCreatedAt)
    {
        return requestedClassification is { IsOfficial: true }
               && yearInfo.LatestFssFileSentAt >= requestedRunCreatedAt;
    }
}
