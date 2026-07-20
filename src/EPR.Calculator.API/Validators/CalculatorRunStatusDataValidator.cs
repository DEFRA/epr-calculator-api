using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunStatusDataValidator : ICalculatorRunStatusDataValidator
    {
        private static readonly Dictionary<RunClassification, RunClassification> ValidTransitions = new()
        {
            [RunClassification.INITIAL_RUN]                         = RunClassification.UNCLASSIFIED,
            [RunClassification.INTERIM_RECALCULATION_RUN]           = RunClassification.UNCLASSIFIED,
            [RunClassification.FINAL_RECALCULATION_RUN]             = RunClassification.UNCLASSIFIED,
            [RunClassification.FINAL_RUN]                           = RunClassification.UNCLASSIFIED,
            [RunClassification.INITIAL_RUN_COMPLETED]               = RunClassification.INITIAL_RUN,
            [RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED] = RunClassification.INTERIM_RECALCULATION_RUN,
            [RunClassification.FINAL_RECALCULATION_RUN_COMPLETED]   = RunClassification.FINAL_RECALCULATION_RUN,
            [RunClassification.FINAL_RUN_COMPLETED]                 = RunClassification.FINAL_RUN,
        };

        private static readonly Dictionary<RunClassification, RunClassification[]> InvalidFromStates = new()
        {
            [RunClassification.DELETED]  = [RunClassification.DELETED, RunClassification.INTHEQUEUE],
            [RunClassification.TEST_RUN] = [RunClassification.TEST_RUN, RunClassification.INTHEQUEUE, RunClassification.RUNNING, RunClassification.ERROR, RunClassification.DELETED],
        };

        public GenericValidationResultDto Validate(
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED
                || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED
                || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED)
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
            if (designatedRuns.Count == 0 && runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN)
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
                       $"Another '{RunClassification.INITIAL_RUN}' already have been completed for '{calculatorRun.RelativeYear}'.",
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
                       $"Another '{RunClassification.FINAL_RECALCULATION_RUN}' already have been completed for '{calculatorRun.RelativeYear}'.",
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
                       $"Another '{RunClassification.FINAL_RUN}' already performed for '{calculatorRun.RelativeYear}'.",
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
                       $"Another '{RunClassification.FINAL_RUN}' already have been completed for '{calculatorRun.RelativeYear}'.",
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
                       $"To classified this run you first need to perform '{RunClassification.INITIAL_RUN_COMPLETED}' for '{calculatorRun.RelativeYear}'.",
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
                       $"To classified this as '{RunClassification.FINAL_RUN}' you first need to perform '{RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED}' or '{RunClassification.FINAL_RECALCULATION_RUN_COMPLETED}' for '{calculatorRun.RelativeYear}'.",
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
            var requested = (RunClassification)runStatusUpdateDto.ClassificationId;
            var current = (RunClassification)calculatorRun.CalculatorRunClassificationId;

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
                && (x.RunClassification == RunClassification.INITIAL_RUN
                || x.RunClassification == RunClassification.INTERIM_RECALCULATION_RUN
                || x.RunClassification == RunClassification.FINAL_RECALCULATION_RUN
                || x.RunClassification == RunClassification.FINAL_RUN))
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN);
        }

        private static bool IsInitialRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.INITIAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED);
        }

        private static bool IsFinalRecalculationRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED);
        }

        private static bool IsFinalRecalculationRunRequestedButFinalRunCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassification == RunClassification.FINAL_RUN_COMPLETED))
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED);
        }

        private static bool IsFinalRunRequestedButCompleted(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.FINAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED);
        }

        private static bool IsRecalculationRequestedBeforeInitialRun(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassification == RunClassification.INITIAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN);
        }

        private static bool IsFinalRunRequestedBeforeRecalculation(List<CalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassification == RunClassification.INITIAL_RUN_COMPLETED
                || x.RunClassification == RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                || x.RunClassification == RunClassification.FINAL_RECALCULATION_RUN_COMPLETED))
                && runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN;
        }

        private static bool IsRequestingClassificationOfOlderRun(List<CalculatorRunDto> designatedRuns, CalculatorRun calculatorRun, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return (runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED)
                && IsCurrentRunOlderThanOtherCompletedRuns(designatedRuns, calculatorRun);
        }

        private static bool IsCurrentRunOlderThanOtherCompletedRuns(
            List<CalculatorRunDto> designatedRuns,
            CalculatorRun calculatorRun)
        {
            return designatedRuns
                .Where(run => run.BillingFile?.HasBeenSentToFss == true)
                .All(run => run.BillingFile!.SentAt.GetValueOrDefault() >= calculatorRun.CreatedAt);
        }
    }
}
