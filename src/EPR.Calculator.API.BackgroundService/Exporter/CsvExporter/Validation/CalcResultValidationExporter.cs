using System.Text;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.BackgroundService.Models;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;

public interface ICalcResultValidationExporter
{
    /// <summary>
    ///     Writes a 'Warning' section to the CSV content if there are any validation failures with
    ///     <see cref="Severity.Warning" />; other severities are ignored.
    /// </summary>
    void ExportWarnings(CalcResult calcResult, StringBuilder csvContent);
}

public class CalcResultValidationExporter(IValidator<CalcResult> validator) : ICalcResultValidationExporter
{
    public void ExportWarnings(CalcResult calcResult, StringBuilder csvContent)
    {
        var validationResult = validator.Validate(calcResult);

        if (validationResult.IsValid)
            return;

        var warnings = validationResult.Errors
            .Where(err => err.Severity == Severity.Warning)
            .ToImmutableArray();

        if (warnings.Length > 0)
            WriteSection(csvContent, "Warning", warnings);
    }

    private static void WriteSection(StringBuilder csvContent, string header, ImmutableArray<ValidationFailure> failures)
    {
        csvContent.AppendLine();
        csvContent.AppendLine();
        csvContent.AppendLine(CsvSanitiser.SanitiseData(header));

        foreach (var failure in failures)
            csvContent.AppendLine(CsvSanitiser.SanitiseData(failure.ErrorMessage));
    }
}
