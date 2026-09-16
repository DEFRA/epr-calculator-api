using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators;

public class SetRunClassificationRequestValidator : AbstractValidator<SetRunClassificationRequest>
{
    public SetRunClassificationRequestValidator()
    {
        RuleFor(x => x.RunId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.ClassificationId)
            .IsInEnum()
            .NotEqual(RunClassification.Unknown)
            .WithMessage("Invalid ClassificationId");
    }
}
