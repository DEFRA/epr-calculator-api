using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using FluentValidation;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;

public class CalculatorRunValidator : AbstractValidator<CalculatorRun>
{
    public CalculatorRunValidator()
    {
        RuleFor(run => run.CalculationRunStatus)
            .Must(status => status == CalculationRunStatus.None)
            .WithMessage($"Run calculation status must be {CalculationRunStatus.None}");

        RuleFor(run => run.Classification)
            .Must(classification => classification == RunClassification.None)
            .WithMessage($"Run classification must be {RunClassification.None}");

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
    }
}
