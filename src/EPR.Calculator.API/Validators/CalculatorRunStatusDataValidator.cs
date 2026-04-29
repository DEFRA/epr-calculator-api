using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunStatusDataValidator : ICalculatorRunStatusDataValidator
    {
        private static readonly Dictionary<RunClassification, RunClassification> ValidTransitions = new()
        {
            [RunClassification.InitialRun]                       = RunClassification.Unclassified,
            [RunClassification.InterimRecalculationRun]          = RunClassification.Unclassified,
            [RunClassification.FinalRecalculationRun]            = RunClassification.Unclassified,
            [RunClassification.FinalRun]                         = RunClassification.Unclassified,
            [RunClassification.InitialRunCompleted]              = RunClassification.InitialRun,
            [RunClassification.InterimRecalculationRunCompleted] = RunClassification.InterimRecalculationRun,
            [RunClassification.FinalRecalculationRunCompleted]   = RunClassification.FinalRecalculationRun,
            [RunClassification.FinalRunCompleted]                = RunClassification.FinalRun,
        };

        private static readonly Dictionary<RunClassification, RunClassification[]> InvalidFromStates = new()
        {
            [RunClassification.Deleted]  = [RunClassification.Deleted, RunClassification.None],
            [RunClassification.TestRun] = [RunClassification.TestRun, RunClassification.None, RunClassification.Running, RunClassification.Errored, RunClassification.Deleted],
        };

        public GenericValidationResultDto Validate(
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            if (calculatorRun.Classification is
                    RunClassification.InitialRunCompleted or
                    RunClassification.InterimRecalculationRunCompleted or
                    RunClassification.FinalRecalculationRunCompleted or
                    RunClassification.FinalRunCompleted)
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                        "Cannot reclassify a run once the run is completed.",
                    ],
                };
            }

            return ValidateClassificationTransition(calculatorRun, runStatusUpdateDto);
        }

        public GenericValidationResultDto Validate(
            List<CalculatorRunDto> designatedRuns,
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            if (designatedRuns.Count == 0 && runStatusUpdateDto.Classification == RunClassification.InitialRun)
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = false,
                };
            }

            if (IsRunAlreadyClassified(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       "Another run already have been classified.",
                    ],
                };
            }
            else if (IsInitialRunRequestedButCompleted(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"Another '{RunClassification.InitialRun}' already have been completed for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsFinalRecalculationRunRequestedButCompleted(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"Another '{RunClassification.FinalRecalculationRun}' already have been completed for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsFinalRecalculationRunRequestedButFinalRunCompleted(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"Another '{RunClassification.FinalRun}' already performed for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsFinalRunRequestedButCompleted(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"Another '{RunClassification.FinalRun}' already have been completed for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsRecalculationRequestedBeforeInitialRun(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"To classified this run you first need to perform '{RunClassification.InitialRunCompleted}' for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsFinalRunRequestedBeforeRecalculation(designatedRuns, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"To classified this as '{RunClassification.FinalRun}' you first need to perform '{RunClassification.InterimRecalculationRunCompleted}' or '{RunClassification.FinalRecalculationRunCompleted}' for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }
            else if (IsRequestingClassificationOfOlderRun(designatedRuns, calculatorRun, runStatusUpdateDto))
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors =
                    [
                       $"You can't classified older run as designated run for '{calculatorRun.RelativeYear}'.",
                    ],
                };
            }

            return new GenericValidationResultDto
            {
                IsInvalid = false,
            };
        }

        private static GenericValidationResultDto ValidateClassificationTransition(CalculatorRun calculatorRun, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            var requested = runStatusUpdateDto.Classification;
            var current = calculatorRun.Classification;

            if (ValidTransitions.TryGetValue(requested, out var requiredCurrent))
            {
                return current == requiredCurrent
                    ? ValidResultDto()
                    : InvalidResultDto(requested);
            }

            if (InvalidFromStates.TryGetValue(requested, out var invalidStates))
            {
                return invalidStates.Contains(current)
                    ? InvalidResultDto(requested)
                    : ValidResultDto();
            }

            return new GenericValidationResultDto
            {
                IsInvalid = true,
                Errors = ["Invalid Classification"],
            };
        }

        private static GenericValidationResultDto ValidResultDto() => new() { IsInvalid = false };

        private static GenericValidationResultDto InvalidResultDto(RunClassification classification) => new()
        {
            IsInvalid = true,
            Errors = [string.Format(CommonResources.InvalidClassification, classification)],
        };

        private static bool IsRunAlreadyClassified(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassification == RunClassification.InitialRun
                || x.RunClassification == RunClassification.InterimRecalculationRun
                || x.RunClassification == RunClassification.FinalRecalculationRun
                || x.RunClassification == RunClassification.FinalRun))
                && (runStatusUpdateDto.Classification == RunClassification.InitialRun
                || runStatusUpdateDto.Classification == RunClassification.InterimRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRun);
        }

        private static bool IsInitialRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.InitialRunCompleted)
                && (runStatusUpdateDto.Classification == RunClassification.InitialRun
                || runStatusUpdateDto.Classification == RunClassification.InitialRunCompleted);
        }

        private static bool IsFinalRecalculationRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.FinalRecalculationRunCompleted)
                && (runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRunCompleted);
        }

        private static bool IsFinalRecalculationRunRequestedButFinalRunCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassification == RunClassification.FinalRunCompleted))
                && (runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRunCompleted);
        }

        private static bool IsFinalRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.FinalRunCompleted)
                && (runStatusUpdateDto.Classification == RunClassification.FinalRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRunCompleted);
        }

        private static bool IsRecalculationRequestedBeforeInitialRun(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.InitialRunCompleted)
                && (runStatusUpdateDto.Classification == RunClassification.InterimRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRun);
        }

        private static bool IsFinalRunRequestedBeforeRecalculation(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassification == RunClassification.InitialRunCompleted
                || x.RunClassification == RunClassification.InterimRecalculationRunCompleted
                || x.RunClassification == RunClassification.FinalRecalculationRunCompleted))
                && runStatusUpdateDto.Classification == RunClassification.FinalRun;
        }

        private static bool IsRequestingClassificationOfOlderRun(List<CalculatorRunDto> designatedRuns, CalculatorRun calculatorRun, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return (runStatusUpdateDto.Classification == RunClassification.InitialRun
                || runStatusUpdateDto.Classification == RunClassification.InitialRunCompleted
                || runStatusUpdateDto.Classification == RunClassification.InterimRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.InterimRecalculationRunCompleted
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRecalculationRunCompleted
                || runStatusUpdateDto.Classification == RunClassification.FinalRun
                || runStatusUpdateDto.Classification == RunClassification.FinalRunCompleted)
                && IsCurrentRunOlderThanOtherCompletedRuns(designatedRuns, calculatorRun);
        }

        private static bool IsCurrentRunOlderThanOtherCompletedRuns(
            List<CalculatorRunDto> designatedRuns,
            CalculatorRun calculatorRun)
        {
            return designatedRuns
                .Where(run => run.CompletedBillingRun?.AuthorisedAt != null)
                .All(run => run.CompletedBillingRun!.AuthorisedAt >= calculatorRun.CreatedAt);
        }
    }
}
