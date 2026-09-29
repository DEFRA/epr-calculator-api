using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
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
            .SetValidator(ctx => new RunValidator(ctx.StartedAt));
    }

    private sealed class RunValidator : AbstractValidator<CalculatorRun>
    {
        public RunValidator(DateTimeOffset startedAt)
        {
            RuleFor(run => run.Classification)
                .Must(classification => classification is RunClassification.Initial or RunClassification.Recalculation)
                .WithMessage("Run must have an official classification.");

            RuleFor(run => run.CalculationRunStatus)
                .Must(status => status == CalculationRunStatus.Completed)
                .WithMessage($"Run calculation status must be {CalculationRunStatus.Completed}");

            RuleFor(run => run.BillingRunStatus)
                .NotEqual(BillingRunStatus.Unknown)
                .WithMessage($"Run billing status must not be {BillingRunStatus.Unknown}");

            // A billing file may be regenerated (e.g. once outdated by changes to billing instructions) or retried,
            // just not whilst a billing run is already underway. Billing runs Started over an hour ago are considered
            // stuck, so may also be restarted.
            RuleFor(run => run.BillingRunStatus)
                .Must((run, _) => !run.IsBillingRunUnderway(startedAt))
                .WithMessage("Run already has a billing run underway");

            RuleFor(run => run.IsBillingFileShared)
                .Must(isBillingFileShared => !isBillingFileShared)
                .WithMessage("Run billing file has already been shared.");

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
        }
    }
}
