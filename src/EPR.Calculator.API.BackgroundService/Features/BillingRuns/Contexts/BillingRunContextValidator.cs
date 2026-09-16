using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;
using FluentValidation;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;

/// <summary>
///     Ensures the state of the CalculatorRun database entity is valid for billing file generation.
/// </summary>
public class BillingRunContextValidator : AbstractValidator<BillingRunContextBuilder.PreValidationContext>
{
    public BillingRunContextValidator()
    {
        RuleFor(ctx => ctx.User)
            .NotEmpty();

        RuleFor(ctx => ctx.StartedAt)
            .NotEqual(DateTimeOffset.MinValue);

        RuleFor(ctx => ctx.AcceptedProducerIds)
            .NotEmpty()
            .WithMessage("No producers have been accepted for this billing run");

        RuleFor(ctx => ctx.Run)
            .SetValidator(new RunValidator());
    }

    private sealed class RunValidator : AbstractValidator<CalculatorRun>
    {
        public RunValidator()
        {
            RuleFor(run => run.Name)
                .NotEmpty()
                .WithMessage("Run has no name");

            RuleFor(run => run.DefaultParameterSettingMasterId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"Run is missing {nameof(DefaultParameterSettingMaster)}");

            RuleFor(run => run.LapcapDataMasterId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"Run is missing {nameof(LapcapDataMaster)}");

            RuleFor(run => run.CalculatorRunOrganisationDataMasterId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"Run is missing {nameof(CalculatorRunOrganisationDataMaster)}");

            RuleFor(run => run.CalculatorRunPomDataMasterId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage($"Run is missing {nameof(CalculatorRunPomDataMaster)}");

            RuleFor(run => run.Classification)
                .Must(classification => classification is { IsOfficial: true, IsCompleted: false })
                .WithMessage("Run must have an Official classification that is not already completed.");

            RuleFor(run => run.BillingRunStatus)
                .Equal(BillingRunStatus.Running)
                .WithMessage($"Status must be '{nameof(BillingRunStatus.Running)}'");
        }
    }
}
