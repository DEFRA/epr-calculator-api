using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.DataTypes;

/// <summary>
///     Represents the various states that a run can transition through during the billing run lifecycle.
/// </summary>
/// <remarks>
///     ⚠️ This is persisted as a string, do NOT use numeric representation/comparison as values may change.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingRunStatus
{
    /// <summary>
    ///     Indicates that the enum value itself was unknown/invalid.
    /// </summary>
    /// <remarks>
    ///     This shouldn't be encountered in practice; it means the given enum value was not one of the below.
    ///     This typically occurs when the value was corrupted/omitted during (de)serialization across boundaries.
    /// </remarks>
    Unknown = 0,

    /// <summary>
    ///     Indicates that the billing run has yet to be attempted.
    /// </summary>
    /// <remarks>
    ///     Note that the run may not yet be in a valid state to actually start the billing run just yet;
    ///     e.g. the calculation run needs to have completed with the correct classification.
    /// </remarks>
    None,

    /// <summary>
    ///     Indicates that the billing run is currently underway.
    /// </summary>
    /// <remarks>
    ///     If the billing run was interrupted unexpectedly, the run may become stuck in this state.
    ///     Change this status back to <see cref="None" />, then restart the billing run.
    ///     This should be safe - database changes aren't committed until the very end of the billing run.
    ///     However: there may now be orphaned billing files for this run in blob storage (harmless, but should be cleaned up).
    /// </remarks>
    Running,

    /// <summary>
    ///     Indicates that the billing run has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Indicates that the billing run has failed.
    /// </summary>
    /// <remarks>
    ///     If the billing run is in this state, it may be retried later.
    ///     Change this status back to <see cref="None" />, then restart the billing run.
    ///     This should be safe - database changes aren't committed until the very end of the billing run.
    ///     However: there may now be orphaned billing files for this run in blob storage (harmless, but should be cleaned up).
    /// </remarks>
    Errored
}
