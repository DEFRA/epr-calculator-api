using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.BackgroundService.Features.RunImport;

/// <summary>
///     What an imported run needs that neither its billing CSV nor its file name holds.
/// </summary>
public record RunImportDetails
{
    /// <summary>Defaults to Initial for a year's first run, and Recalculation for the runs after it.</summary>
    public RunClassification? Classification { get; init; }

    /// <summary>Who created the billing file; defaults to who ran the calculation (the file's "Run by").</summary>
    public string? BillingFileCreatedBy { get; init; }

    /// <summary>When the billing file was sent to FSS; defaults to when it was created.</summary>
    public DateTime? BillingFileSharedAt { get; init; }

    public string? BillingFileSharedBy { get; init; }

    /// <summary>
    ///     Default parameters the file doesn't show, by their parameter reference (for example "LRET-AL-R").
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultParameters { get; init; } = new Dictionary<string, string>();
}
