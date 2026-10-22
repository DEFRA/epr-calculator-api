using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Api.DataApi;

/// <summary>
///     Configuration for selecting submissions, bound from <c>CalculatorDataApi</c>.
/// </summary>
internal sealed record CalculatorDataApiOptions
{
    public const string SectionKey = "CalculatorDataApi";

    /// <summary>
    ///     The month of the relative year after which a Pending original submission counts as late and is
    ///     included, e.g. 12 includes those created after 31 December of the relative year.
    /// </summary>
    [Range(1, 12)]
    public int LateSubmissionsAfterMonth { get; init; }
}
