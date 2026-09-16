namespace EPR.Calculator.API.Data.Enums;

/// <remarks>
///     Numbering is based on the database enum. There are some gaps due to the removal of obsoleted legacy values.
/// </remarks>
public enum RunClassification
{
    // --- System designated classifications -----------------------------
    None                   = 0,
    Running                = 2,
    Unclassified           = 3,
    Errored                = 5,
    Deleted                = 6,

    // --- User designated classifications -------------------------------

    /// <summary>
    ///     Marks a run as a non-Official test run. These are essentially ignored, especially from processes that
    ///     aggregate across a year.
    /// </summary>
    Test                   = 4,

    /// <summary>
    ///     Marks a run as Officially classified. It is only applicable to the first run of the year.
    /// </summary>
    Initial                = 8,

    /// <summary>
    ///     Marks a run as Officially classified. It is only applicable to any additional runs within a year following
    ///     the <see cref="Initial" /> run.
    /// </summary>
    Recalculation          = 9,

    // --- Completed classifications -------------------------------------

    /// <summary>
    ///     Marks a <see cref="Initial" /> run as having had a Billing File generated and sent to FSS.
    /// </summary>
    InitialCompleted       = 7,

    /// <summary>
    ///     Marks a <see cref="Recalculation" /> run as having had a Billing File generated and sent to FSS.
    /// </summary>
    RecalculationCompleted = 12
}

public static class RunClassificationHelper
{
    /// <summary>
    ///     Completed classifications are all <see cref="OfficialClassifications">Official</see> classifications that
    ///     have had a Billing File generated and sent to FSS.
    /// </summary>
    public static readonly ImmutableHashSet<RunClassification> CompletedClassifications =
    [
        RunClassification.InitialCompleted,
        RunClassification.RecalculationCompleted
    ];

    /// <summary>
    ///     Official classifications are all non-<see cref="RunClassification.Test">Test</see> classifications that
    ///     may have been designated by a user.
    /// </summary>
    /// <remarks>
    ///     This includes Completed Official classifications.
    /// </remarks>
    public static readonly ImmutableHashSet<RunClassification> OfficialClassifications =
    [
        RunClassification.Initial,
        RunClassification.Recalculation,
        .. CompletedClassifications
    ];

    extension(RunClassification classification)
    {
        /// <summary>
        ///     Has the run been given an <see cref="OfficialClassifications">Official</see> classification?
        /// </summary>
        public bool IsOfficial => OfficialClassifications.Contains(classification);

        /// <summary>
        ///     Has the run been sent to FSS?
        /// </summary>
        public bool IsCompleted => CompletedClassifications.Contains(classification);
    }
}
