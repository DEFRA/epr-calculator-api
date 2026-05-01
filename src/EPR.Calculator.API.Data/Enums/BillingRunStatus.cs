using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.Enums;

/// <summary>
///     Represents the various states that a run can transition through during the billing process lifecycle.
/// </summary>
/// <remarks>
///     ⚠️ This is persisted as a string, do NOT use numeric representation/comparison as that may change.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingRunStatus
{
    /// <summary>
    ///     Indicates that the run is in an unknown state regarding the billing process.
    /// </summary>
    /// <remarks>
    ///     This should never be encountered in practice; it probably means bad data has been stored in the database.
    /// </remarks>
    Unknown = 0,

    /// <summary>
    ///     Indicates that the run is in a state where the billing process has yet to be attempted.
    /// </summary>
    /// <remarks>
    ///     Note that the run may not be in a valid state to start the billing process, e.g. the calculation part of the run
    ///     still needs to be completed.
    /// </remarks>
    None,

    /// <summary>
    ///     Indicates that a signal has been sent for the billing process to start.
    /// </summary>
    /// <remarks>
    ///     This should only be active for a short window before the service bus message is picked up, and mainly exists
    ///     to prevent users issuing duplicate requests.
    /// </remarks>
    Starting,

    /// <summary>
    ///     Indicates that the billing process is currently underway.
    /// </summary>
    /// <remarks>
    ///     If the billing process was interrupted unexpectedly, the run may become stuck in this state.
    ///     Change this status back to <see cref="None" />, then restart the billing process.
    ///     This should be safe - database changes aren't committed until the very end of processing.
    ///     However: there may now be orphaned billing files for this run in blob storage (harmless, but should be cleaned up).
    /// </remarks>
    Running,

    /// <summary>
    ///     Indicates that the billing process completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Indicates that the billing process failed.
    /// </summary>
    /// <remarks>
    ///     If the billing run is in this state, it may be retried - database changes aren't committed until the very end of
    ///     processing.
    ///     However: there may now be orphaned billing files for this run in blob storage (harmless, but should be cleaned up).
    /// </remarks>
    Errored
}
