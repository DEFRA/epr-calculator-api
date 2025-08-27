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
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.DELETED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = true,
                            Errors =
                            [
                                string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN),
                            ],
                        };
                    }

                    return new GenericValidationResultDto
                    {
                        IsInvalid = false,
                    };
                case (int)RunClassification.TEST_RUN:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.TEST_RUN)
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
    }
}
