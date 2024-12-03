using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators;

public class CalculatorRunStatusUpdateValidator : AbstractValidator<CalculatorRunStatusUpdateDto>
{
    public CalculatorRunStatusUpdateValidator()
    {
        RuleFor(x => x.ClassificationId).GreaterThan(0).WithMessage("Invalid ClassificationId");
        RuleFor(x => x.RunId).GreaterThan(0).WithMessage("Invalid RunId");
    }
}