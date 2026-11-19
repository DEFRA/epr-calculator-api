using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.Services.RunImport;

/// <summary>
///     What an imported run needs that neither its results and billing CSVs nor their file names hold.
/// </summary>
public record RunImportDetails
{
    /// <summary>Defaults to Initial for a year's first run, and Recalculation for the runs after it.</summary>
    public RunClassification? Classification { get; init; }

    /// <summary>When the run was made, to the second; the files only show the minute.</summary>
    public DateTime? RunDate { get; init; }

    /// <summary>Who created the billing file; defaults to who ran the calculation (the file's "Run by").</summary>
    public string? BillingFileCreatedBy { get; init; }

    /// <summary>When the billing file was sent to FSS; defaults to when it was created.</summary>
    public DateTime? BillingFileSharedAt { get; init; }

    public string? BillingFileSharedBy { get; init; }

    /// <summary>
    ///     Names as stored, by producer and subsidiary ID, for the names the files lost commas or spaces from.
    /// </summary>
    public IReadOnlyDictionary<(int ProducerId, string? SubsidiaryId), ProducerNames> ProducerNames { get; init; } =
        new Dictionary<(int ProducerId, string? SubsidiaryId), ProducerNames>();

    /// <summary>
    ///     Who accepted or rejected each producer's billing instruction, and when, by producer ID. Without these, the
    ///     billing file's producers count as accepted and its Rejected Report's as rejected.
    /// </summary>
    public IReadOnlyDictionary<int, BillingDecision> BillingDecisions { get; init; } = new Dictionary<int, BillingDecision>();

    /// <summary>
    ///     Default parameters the file doesn't show, by their parameter reference (for example "LRET-AL-R").
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultParameters { get; init; } = new Dictionary<string, string>();
}

public sealed record ProducerNames(string? ProducerName, string? TradingName);

public sealed record BillingDecision(string AcceptReject, string? DecidedBy, DateTime? DecidedAt);
