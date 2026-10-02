using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.DataTypes;

/// <summary>
///     Represents the user-assigned classification of a run.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunClassification
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
    ///     Run has yet to be classified; classification can only occur once the run has been completed.
    /// </summary>
    None,

    /// <summary>
    ///     Marks a completed run as a non-Official test run. These are essentially ignored, especially from processes that
    ///     aggregate across a year.
    /// </summary>
    Test,

    /// <summary>
    ///     Marks a completed run as Officially classified. It is only applicable to the first run of the year.
    /// </summary>
    Initial,

    /// <summary>
    ///     Marks a completed run as Officially classified. It is only applicable to any additional runs within a year following
    ///     the <see cref="Initial" /> run.
    /// </summary>
    Recalculation,

    /// <summary>
    ///     Marks a completed run as having been deleted.
    /// </summary>
    Deleted
}
