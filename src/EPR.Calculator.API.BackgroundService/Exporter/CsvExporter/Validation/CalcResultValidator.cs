using EPR.Calculator.API.BackgroundService.Models;
using FluentValidation;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;

public class CalcResultValidator : AbstractValidator<CalcResult>
{
    public CalcResultValidator()
    {
        RuleFor(calcResult => calcResult.CalcResultModulation)
            .SetValidator(new ModulationResultValidator()!)
            .When(x => x.CalcResultModulation is not null);
    }
}
