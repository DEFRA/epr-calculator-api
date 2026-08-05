using EPR.Calculator.Service.Function.Models;
using FluentValidation;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Validation;

public class CalcResultValidator : AbstractValidator<CalcResult>
{
    public CalcResultValidator()
    {
        RuleFor(calcResult => calcResult.CalcResultModulation)
            .SetValidator(new ModulationResultValidator()!)
            .When(x => x.CalcResultModulation is not null);
    }
}
