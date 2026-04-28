using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunStatusDataValidator : ICalculatorRunStatusDataValidator
    {
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

            switch (runStatusUpdateDto.ClassificationId)
            {
                case (int)RunClassification.INITIAL_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.UNCLASSIFIED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN),
                        ],
                    };
                case (int)RunClassification.INITIAL_RUN_COMPLETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INITIAL_RUN)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN_COMPLETED),
                        ],
                    };
                case (int)RunClassification.INTERIM_RECALCULATION_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.UNCLASSIFIED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.INTERIM_RECALCULATION_RUN),
                        ],
                    };
                case (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED),
                        ],
                    };
                case (int)RunClassification.FINAL_RECALCULATION_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.UNCLASSIFIED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.FINAL_RECALCULATION_RUN),
                        ],
                    };
                case (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED),
                        ],
                    };
                case (int)RunClassification.FINAL_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.UNCLASSIFIED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.FINAL_RUN),
                        ],
                    };
                case (int)RunClassification.FINAL_RUN_COMPLETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.FINAL_RUN)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = false,
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            string.Format(CommonResources.InvalidClassification, RunClassification.FINAL_RUN_COMPLETED),
                        ],
                    };
                case (int)RunClassification.DELETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.DELETED
                        || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INTHEQUEUE)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = true,
                            Errors =
                            [
                                string.Format(CommonResources.InvalidClassification, RunClassification.DELETED),
                            ],
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = false,
                    };
                case (int)RunClassification.TEST_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.TEST_RUN
                        || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INTHEQUEUE
                        || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.RUNNING
                        || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.ERROR
                        || calculatorRun.CalculatorRunClassificationId == (int)RunClassification.DELETED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = true,
                            Errors =
                            [
                                string.Format(CommonResources.InvalidClassification, RunClassification.TEST_RUN),
                            ],
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = false,
                    };
                default:
                    return new GenericValidationResultDto
                    {
                        IsInvalid = true,
                        Errors =
                        [
                            "Invalid Classification",
                        ],
                    };
            }
        }

        public GenericValidationResultDto Validate(
            List<ClassifiedCalculatorRunDto> designatedRuns,
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

        private static bool IsRunAlreadyClassified(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassificationId == (int)RunClassification.INITIAL_RUN
                || x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || x.RunClassificationId == (int)RunClassification.FINAL_RUN))
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN);
        }

        private static bool IsInitialRunRequestedButCompleted(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED);
        }

        private static bool IsFinalRecalculationRunRequestedButCompleted(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED);
        }

        private static bool IsFinalRecalculationRunRequestedButFinalRunCompleted(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED))
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED);
        }

        private static bool IsFinalRunRequestedButCompleted(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED);
        }

        private static bool IsRecalculationRequestedBeforeInitialRun(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED)
                && (runStatusUpdateDto.ClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
                || runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN);
        }

        private static bool IsFinalRunRequestedBeforeRecalculation(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            return !designatedRuns.Exists(x => x.RunId != runStatusUpdateDto.RunId
                && (x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED
                || x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                || x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED))
                && runStatusUpdateDto.ClassificationId == (int)RunClassification.FINAL_RUN;
        }

        private static bool IsRequestingClassificationOfOlderRun(List<ClassifiedCalculatorRunDto> designatedRuns, CalculatorRun calculatorRun, CalculatorRunStatusUpdateDto runStatusUpdateDto)
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
            List<ClassifiedCalculatorRunDto> designatedRuns,
            CalculatorRun calculatorRun)
        {
            return designatedRuns.Where(run => run.BillingFileAuthorisedDate.HasValue)
                                 .All(run => (run.BillingFileAuthorisedDate!.Value >= calculatorRun.CreatedAt));
        }
    }
}
