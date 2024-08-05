using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class ParameterYearValueValidationValidator : AbstractValidator<string>
    {
        public ParameterYearValueValidationValidator()
        {
            RuleFor(x => x).NotEmpty().WithMessage("Parameter Year is required");
        }
    }
}