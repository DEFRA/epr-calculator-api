using EPR.Calculator.API.Data.DataModels;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Validation;

public class ModulationResultValidator : AbstractValidator<ModulationResult>
{
    public ModulationResultValidator()
    {
        RuleFor(x => x.GreenFactor)
            .GreaterThanOrEqualTo(0)
            .WithSeverity(Severity.Warning)
            .WithMessage("The green modulation factor is negative. This will result in negative green disposal costs for all materials. It may also result in negative total disposal costs for producers. Check that the data is correct before continuing");
    }
}
