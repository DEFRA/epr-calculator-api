using EPR.Calculator.API.Data.DataModels;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunValidator : AbstractValidator<string>
    {
        public CalculatorRunValidator()
        {
            this.RuleFor(x => x).NotEmpty().WithMessage(CommonResources.CalculatorRunNameRequired);
        }

        public ValidationResult ValidateCalculatorRunIds(CalculatorRun calculatorRun)
        {
            var errorMessages = new List<string>();

            var requiredMasterIds = new Dictionary<string, int?>
            {
                { "CalculatorRunOrganisationDataMasterId", calculatorRun.CalculatorRunOrganisationDataMasterId },
                { "DefaultParameterSettingMasterId", calculatorRun.DefaultParameterSettingMasterId },
                { "CalculatorRunPomDataMasterId", calculatorRun.CalculatorRunPomDataMasterId },
                { "LapcapDataMasterId", calculatorRun.LapcapDataMasterId },
            };

            errorMessages.AddRange(requiredMasterIds
                .Where(id => id.Value == null)
                .Select(id => $"{id.Key} is null"));

            return new ValidationResult
            {
                IsValid = errorMessages.Count == 0,
                ErrorMessages = errorMessages,
            };
        }
    }
}