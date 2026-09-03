using System.Globalization;

namespace EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;

/// <summary>
///     Classifies a POM submission_period string ("2024-P1".."2024-P4", or "&lt;year&gt;-H1"/"-H2" for
///     years after 2024) into the half of the year it counts towards, ported from the has_h1/has_h2
///     CASE expressions that previously lived in the Paycal org/POM stored procedures.
/// </summary>
internal static class SubmissionPeriodClassification
{
    public static bool TryParseYear(string? submissionPeriod, out int year)
    {
        year = 0;
        return submissionPeriod is { Length: >= 4 } &&
               int.TryParse(submissionPeriod.AsSpan(0, 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out year);
    }

    public static bool IsH1(string submissionPeriod, int year) =>
        (year > 2024 && submissionPeriod.EndsWith("-H1", StringComparison.Ordinal)) ||
        submissionPeriod is "2024-P1" or "2024-P2" or "2024-P3";

    public static bool IsH2(string submissionPeriod, int year) =>
        (year > 2024 && submissionPeriod.EndsWith("-H2", StringComparison.Ordinal)) ||
        submissionPeriod == "2024-P4";
}
