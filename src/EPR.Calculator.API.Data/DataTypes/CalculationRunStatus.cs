using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.DataTypes;

/// <summary>
///     Represents the various states that a run can transition through during the calculation run lifecycle.
/// </summary>
/// <remarks>
///     ⚠️ This is persisted as a string, do NOT use numeric representation/comparison as values may change.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CalculationRunStatus
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
    ///     Indicates that the calculation run has yet to be attempted.
    /// </summary>
    None,

    /// <summary>
    ///     Indicates that the calculation run has started and should be currently underway.
    /// </summary>
    /// <remarks>
    ///     If the calculation run was interrupted unexpectedly, the run may become stuck in this state.
    ///     Change this status to <see cref="Errored" />, then start a new calculation run.
    /// </remarks>
    Started,

    /// <summary>
    ///     Indicates that the calculation run has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    ///     Indicates that the calculation run has failed.
    /// </summary>
    Errored
}
