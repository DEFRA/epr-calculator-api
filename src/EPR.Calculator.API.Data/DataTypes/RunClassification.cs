namespace EPR.Calculator.API.Data.DataTypes;

/// <remarks>
///     Numbering is based on the database enum. There are some gaps due to the removal of obsoleted legacy values.
/// </remarks>
public enum RunClassification
{
    Unknown                = 0,
    Running                = 2,
    Unclassified           = 3,
    Test                   = 4,
    Errored                = 5,
    Deleted                = 6,
    Initial                = 8,
    InitialCompleted       = 7,
    Recalculation          = 9,
    RecalculationCompleted = 12
}

public static class RunClassificationHelper
{
    public static readonly ImmutableHashSet<RunClassification> CompletedClassifications =
    [
        RunClassification.InitialCompleted,
        RunClassification.RecalculationCompleted
    ];

    public static readonly ImmutableHashSet<RunClassification> DesignatedClassifications =
    [
        RunClassification.Initial,
        RunClassification.Recalculation,
        .. CompletedClassifications
    ];

    extension(RunClassification classification)
    {
        /// <summary>
        ///     Has the run been given a 'proper' classification?
        /// </summary>
        public bool IsDesignated => DesignatedClassifications.Contains(classification);

        /// <summary>
        ///     Has the run been sent to FSS?
        /// </summary>
        public bool IsCompleted => CompletedClassifications.Contains(classification);
    }
}
