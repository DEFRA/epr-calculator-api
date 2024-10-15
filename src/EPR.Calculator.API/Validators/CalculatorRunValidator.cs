using EPR.Calculator.API.Constants;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunValidator : AbstractValidator<string>
    {
        public CalculatorRunValidator()
        {
            RuleFor(x => x).NotEmpty().WithMessage("Calculator Run Name is Required");
        }
    }
}