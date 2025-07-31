using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class ParameterYearValueValidationValidator : AbstractValidator<string>
    {
        public ParameterYearValueValidationValidator()
        {
            this.RuleFor(x => x).NotEmpty().WithMessage(CommonResources.ParameterYearRequired);
        }
    }
}