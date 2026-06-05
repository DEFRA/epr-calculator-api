using EPR.Calculator.API.Data.DataModels;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;

public class CalculatorRunValidator : AbstractValidator<API.Data.DataModels.CalculatorRun>
{
    private static readonly ImmutableHashSet<int> ValidClassifications = [
        RunClassificationStatusIds.INTHEQUEUEID,
        RunClassificationStatusIds.RUNNINGID
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

        RuleFor(run => run.CalculatorRunClassificationId)
            .Must(classification => ValidClassifications.Contains(classification))
            .WithMessage($"Run classification must be one of [{string.Join(", ", ValidClassifications)}]");
    }
}
