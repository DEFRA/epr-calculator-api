using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using FluentValidation;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;

public class CalculatorRunValidator : AbstractValidator<CalculatorRun>
{
    private static readonly ImmutableHashSet<RunClassification> ValidClassifications = [
        RunClassification.Running
    ];

    public CalculatorRunValidator()
    {
        RuleFor(run => run.Name)
            .NotEmpty()
            .WithMessage("Run has no name");

        RuleFor(run => run.DefaultParameterSettingMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing ParameterSettingMaster");

        RuleFor(run => run.LapcapDataMasterId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Run is missing LapcapDataMaster");

        RuleFor(run => run.CalculatorRunOrganisationDataMasterId)
            .Null()
            .WithMessage("Run already has OrganisationDataMaster associated");

        RuleFor(run => run.CalculatorRunPomDataMasterId)
            .Null()
            .WithMessage("Run already has PomDataMaster associated");

        RuleFor(run => run.Classification)
            .Must(classification => ValidClassifications.Contains(classification))
            .WithMessage($"Run classification must be one of [{string.Join(", ", ValidClassifications)}]");
    }
}
