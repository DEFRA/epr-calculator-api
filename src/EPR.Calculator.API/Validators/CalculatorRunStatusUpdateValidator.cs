using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators;

public class CalculatorRunStatusUpdateValidator : AbstractValidator<CalculatorRunStatusUpdateDto>
{
    public CalculatorRunStatusUpdateValidator()
    {
        RuleFor(x => x.RunId)
            .GreaterThan(0)
            .WithMessage("Invalid RunId");

        RuleFor(x => x.Classification)
            .IsInEnum()
            .NotEqual(RunClassification.Unknown)
            .WithMessage("Invalid ClassificationId");
    }
}
