﻿using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Wrapper;

namespace EPR.Calculator.API.Validators
{
    public class RpdStatusDataValidator : IRpdStatusDataValidator
    {
        private readonly IOrgAndPomWrapper wrapper;

        public RpdStatusDataValidator(IOrgAndPomWrapper wrapper)
        {
            this.wrapper = wrapper;
        }

        public RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId, IEnumerable<CalculatorRunClassification> calculatorRunClassifications)
        {
            if (calcRun == null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Calculator Run {runId} is missing"
                };
            }

            if (calcRun.CalculatorRunOrganisationDataMasterId != null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} already has OrganisationDataMasterId associated with it"
                };
            }

            if (calcRun.CalculatorRunPomDataMasterId != null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} already has PomDataMasterId associated with it"
                };
            }

            var expectedRunClassifications = calculatorRunClassifications.Where(cl => 
                cl.Status == "RUNNING" || cl.Status == "IN THE QUEUE"
            );

            if(!expectedRunClassifications.Any(cl => cl.Id == calcRun.CalculatorRunClassificationId))
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} classification should be RUNNING or IN THE QUEUE"
                };
            }

            return new RpdStatusValidation
            {
                isValid = true
            };
        }

        public RpdStatusValidation IsValidSuccessfulRun(int runId)
        {
            var pomDataExists = this.wrapper.AnyPomData();
            var organisationDataExists = this.wrapper.AnyOrganisationData();
            if (!pomDataExists || !organisationDataExists)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = "PomData or Organisation Data is missing"
                };
            }

            return new RpdStatusValidation
            {
                isValid = true
            };

        }
    }
}
