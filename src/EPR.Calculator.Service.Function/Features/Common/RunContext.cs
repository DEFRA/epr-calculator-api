using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Features.Common;

/// <summary>
///     Base class for all run contexts, encapsulating common properties derived from the underlying
///     <see cref="CalculatorRun" /> and elsewhere.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract record RunContext
{
    /// <summary>
    ///     The <see cref="Common.RunType">RunType</see> of this run.
    /// </summary>
    public abstract RunType RunType { get; }

    /// <summary>
    ///     The unique identifier of this run.
    /// </summary>
    public required int RunId { get; init; }

    /// <summary>
    ///     The name supplied by the user who initiated this run.
    /// </summary>
    public required string RunName { get; init; }

    /// <summary>
    ///     When this service first started processing this run.
    /// </summary>
    public required DateTimeOffset ProcessingStartedAt { get; init; }

    /// <summary>
    ///     The relative year of this run. Data sources will be queried against this year (e.g. POMs, Organisations).
    /// </summary>
    public required RelativeYear RelativeYear { get; init; }

    /// <summary>
    ///     The user who initiated this run.
    /// </summary>
    public required string User { get; init; }

    /// <summary>
    ///     Whether modulation data is required for this run.
    /// </summary>
    public bool RequiresModulation => RelativeYear.Value >= 2026;

    /// <summary>
    ///     Whether scaled up producer data is required for this run.
    /// </summary>
    public bool RequiresScaling => RelativeYear.Value == 2025;
}
