using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunStatusDataValidator : ICalculatorRunStatusDataValidator
    {
        public CalculatorRunStatusDataValidator()
        {
        }

        public GenericValidationResultDto Validate(
            CalculatorRun calculatorRun,
            CalculatorRunStatusUpdateDto runStatusUpdateDto)
        {
            if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED)
            {
                return new GenericValidationResultDto
                {
                    IsInvalid = true,
                    Errors = new List<string>
                    {
                        "Cannot reclassify a run once the initial run is completed.",
                    },
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
                        Errors = new List<string>
                        {
                            string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN),
                        },
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
                        Errors = new List<string>
                        {
                            string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN_COMPLETED),
                        },
                    };
                case (int)RunClassification.DELETED:
                    if (calculatorRun.CalculatorRunClassificationId == (int)RunClassification.DELETED)
                    {
                        return new GenericValidationResultDto
                        {
                            IsInvalid = true,
                            Errors = new List<string>
                            {
                                string.Format(CommonResources.InvalidClassification, RunClassification.INITIAL_RUN),
                            },
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
                        Errors = new List<string>
                        {
                            "Invalid Classification",
                        },
                    };
            }
        }
    }
}
