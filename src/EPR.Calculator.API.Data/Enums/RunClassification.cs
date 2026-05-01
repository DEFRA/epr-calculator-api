using System.Text.Json.Serialization;

namespace EPR.Calculator.API.Data.Enums;

/// <summary>
///     Represents the various states that a run can transition through during the calculation process lifecycle.
/// </summary>
/// <remarks>
///     ⚠️ This is persisted as a string, do NOT use numeric representation/comparison as that may change.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunClassification
{
    Unknown = 0,

    None,

    Running,

    Unclassified,

    TestRun,

    InitialRun,

    InitialRunCompleted,

    InterimRecalculationRun,

    InterimRecalculationRunCompleted,

    FinalRecalculationRun,

    FinalRecalculationRunCompleted,

    FinalRun,

    FinalRunCompleted,

    Deleted,

    Errored
}
