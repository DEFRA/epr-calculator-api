using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.PomEligibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi.AcceptedFileSelection;

/// <summary>
///     The identity of a POM candidate file, without its line items - enough to decide which file wins.
/// </summary>
public sealed record PomFileCandidate(
    int? OrganisationId,
    string? SubmitterId,
    string? SubmissionPeriod,
    string? FileName,
    bool IsResubmission,
    string? RegulatorStatus,
    DateTime? CreatedDateTime);

/// <summary>
///     Picks the winning file per organisation/submitter/period from the candidate rows streamed by
///     StreamOrganisationsRequestHandler/StreamPomsRequestHandler, ported from the cut-off-date logic that
///     previously lived inline in those handlers' SQL. Within each group, the latest eligible file wins:
///     <list type="bullet">
///         <item>Accepted, Granted and Cancelled files are eligible.</item>
///         <item>A Pending file is only eligible when it's an original created after the end of
///         <see cref="CalculatorDataApiOptions.LateSubmissionsAfterMonth" /> in the relative year - a late
///         submission.</item>
///         <item>Rejected, Refused, Queried and Uploaded files are never eligible, nor is a file whose
///         status is missing or unrecognised (logged as a warning).</item>
///         <item>A resubmission created after <paramref name="cutOffDate" /> isn't eligible.</item>
///     </list>
///     So when the latest file isn't eligible the search falls back to the latest one that is, and groups
///     with no eligible candidate are excluded entirely.
///
///     For POM files, "period" is a half-year, not the file's literal submission_period: 2024-P1/P2/P3
///     all compete as one H1 group, and 2024-P4 is H2 on its own (matching how H1/H2 are classified
///     everywhere else - see <see cref="SubmissionPeriodClassification" />). A resubmission for any one
///     of P1/P2/P3 is treated as superseding the others, the same as a resubmission of P4 supersedes an
///     earlier P4 file.
/// </summary>
internal interface IAcceptedFileSelector
{
    IReadOnlyList<PayCalOrganisation> SelectLatestOrganisationFiles(
        IReadOnlyList<PayCalOrganisation> organisations, int relativeYear, DateTimeOffset? cutOffDate);

    IReadOnlyList<PayCalPom> SelectLatestPomFiles(
        IReadOnlyList<PayCalPom> poms, int relativeYear, DateTimeOffset? cutOffDate);

    /// <summary>
    ///     The winning file name per (organisation, submitter, period), decided from candidate file
    ///     metadata alone. Lets a caller stream the POM rows in two passes - one to gather the
    ///     candidates, one to keep only the winners' rows - instead of buffering every row.
    /// </summary>
    IReadOnlyDictionary<(int? OrganisationId, string? SubmitterId, string? SubmissionPeriod), string?>
        SelectWinningPomFileNames(IEnumerable<PomFileCandidate> candidates, int relativeYear, DateTimeOffset? cutOffDate);
}

internal sealed class AcceptedFileSelector(
    IOptions<CalculatorDataApiOptions> options,
    ILogger<AcceptedFileSelector> logger)
    : IAcceptedFileSelector
{
    private DateTime LateSubmissionsFrom(int relativeYear) =>
        new DateTime(relativeYear, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(options.Value.LateSubmissionsAfterMonth);

    private static readonly HashSet<string> RecognisedStatuses =
        ["Accepted", "Granted", "Cancelled", "Pending", "Rejected", "Refused", "Queried", "Uploaded"];

    public IReadOnlyList<PayCalOrganisation> SelectLatestOrganisationFiles(
        IReadOnlyList<PayCalOrganisation> organisations, int relativeYear, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestOrganisationFiles),
            () =>
            {
                WarnOfUnrecognisedStatuses("organisation", organisations, o => o.FileName, o => o.RegulatorStatus);
                var lateSubmissionsFrom = LateSubmissionsFrom(relativeYear);
                return FilterToWinners(
                    organisations,
                    o => (o.OrganisationId, o.SubmitterId, o.SubmissionPeriodYear),
                    o => o.FileName,
                    o => o.CreatedDateTime,
                    o => IsEligible(o.RegulatorStatus, o.IsResubmission, o.CreatedDateTime, lateSubmissionsFrom, cutOffDate));
            });

    public IReadOnlyList<PayCalPom> SelectLatestPomFiles(
        IReadOnlyList<PayCalPom> poms, int relativeYear, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestPomFiles),
            () =>
            {
                WarnOfUnrecognisedStatuses("POM", poms, p => p.FileName, p => p.RegulatorStatus);
                var lateSubmissionsFrom = LateSubmissionsFrom(relativeYear);
                return FilterToWinners(
                    poms,
                    p => (p.OrganisationId, p.SubmitterId, HalfPeriod: ToH1OrH2(p.SubmissionPeriod)),
                    p => p.FileName,
                    p => p.CreatedDateTime,
                    p => IsEligible(p.RegulatorStatus, p.IsResubmission, p.CreatedDateTime, lateSubmissionsFrom, cutOffDate));
            });

    public IReadOnlyDictionary<(int? OrganisationId, string? SubmitterId, string? SubmissionPeriod), string?>
        SelectWinningPomFileNames(IEnumerable<PomFileCandidate> candidates, int relativeYear, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectWinningPomFileNames),
            () =>
            {
                var candidateList = candidates as IReadOnlyList<PomFileCandidate> ?? candidates.ToList();
                WarnOfUnrecognisedStatuses("POM", candidateList, c => c.FileName, c => c.RegulatorStatus);
                var lateSubmissionsFrom = LateSubmissionsFrom(relativeYear);

                var winnerByHalf = WinningFileNames(
                    candidateList,
                    c => (c.OrganisationId, c.SubmitterId, HalfPeriod: ToH1OrH2(c.SubmissionPeriod)),
                    c => c.FileName,
                    c => c.CreatedDateTime,
                    c => IsEligible(c.RegulatorStatus, c.IsResubmission, c.CreatedDateTime, lateSubmissionsFrom, cutOffDate));

                // Re-key by each candidate's own SubmissionPeriod - callers look up a POM's own raw
                // period (e.g. 2024-P1), which needs to resolve to its H1 group's winner, the same
                // winner 2024-P3's own key resolves to. A half with no eligible winner gets no entry,
                // same as WinningFileNames excludes such groups entirely.
                return candidateList
                    .Select(c => (Key: (c.OrganisationId, c.SubmitterId, c.SubmissionPeriod), HalfKey: (c.OrganisationId, c.SubmitterId, HalfPeriod: ToH1OrH2(c.SubmissionPeriod))))
                    .Distinct()
                    .Where(x => winnerByHalf.ContainsKey(x.HalfKey))
                    .ToDictionary(x => x.Key, x => winnerByHalf[x.HalfKey]);
            });

    private static bool IsEligible(string? regulatorStatus, bool isResubmission, DateTime? createdDateTime, DateTime lateSubmissionsFrom, DateTimeOffset? cutOffDate)
    {
        if (isResubmission && cutOffDate is not null && !(createdDateTime <= cutOffDate.Value.UtcDateTime))
            return false;

        return regulatorStatus switch
        {
            // A cancelled registration has to win so the producer is reported as Not Obligated, rather than falling back to an earlier one.
            "Accepted" or "Granted" or "Cancelled" => true,
            "Pending" => !isResubmission && createdDateTime >= lateSubmissionsFrom,
            _ => false
        };
    }

    private void WarnOfUnrecognisedStatuses<T>(string fileType, IEnumerable<T> rows, Func<T, string?> fileNameSelector, Func<T, string?> regulatorStatusSelector)
    {
        var unrecognised = rows
            .Where(r => regulatorStatusSelector(r) is not { } status || !RecognisedStatuses.Contains(status))
            .Select(r => (FileName: fileNameSelector(r), Status: regulatorStatusSelector(r) ?? "(none)"))
            .Distinct()
            .ToList();

        if (unrecognised.Count > 0)
            logger.LogWarning(
                "Excluded {Count} {FileType} files with a missing or unrecognised regulator status: {Statuses}",
                unrecognised.Count, fileType, string.Join(", ", unrecognised.Select(u => u.Status).Distinct()));
    }

    /// <summary>
    ///     Maps a POM submission_period to the half-year it belongs to (e.g. 2024-P1/P2/P3 and any year's
    ///     "-H1" period all become "&lt;year&gt;-H1") so file selection treats them as one competing group.
    ///     Falls back to the literal period for anything <see cref="SubmissionPeriodClassification" /> can't
    ///     classify.
    /// </summary>
    private static string? ToH1OrH2(string? submissionPeriod)
    {
        if (!SubmissionPeriodClassification.TryParseYear(submissionPeriod, out var year))
            return submissionPeriod;

        if (SubmissionPeriodClassification.IsH1(submissionPeriod!, year))
            return $"{year}-H1";

        return SubmissionPeriodClassification.IsH2(submissionPeriod!, year) ? $"{year}-H2" : submissionPeriod;
    }

    private static Dictionary<TKey, string?> WinningFileNames<T, TKey>(
        IEnumerable<T> rows,
        Func<T, TKey> groupKeySelector,
        Func<T, string?> fileNameSelector,
        Func<T, DateTime?> createdDateTimeSelector,
        Func<T, bool> isEligibleSelector)
        where TKey : notnull
    {
        return rows
            .GroupBy(groupKeySelector)
            .Select(group => (
                group.Key,
                Winner: group
                    .Where(isEligibleSelector)
                    .MaxBy(createdDateTimeSelector)))
            .Where(group => group.Winner is not null)
            .ToDictionary(group => group.Key, group => fileNameSelector(group.Winner!));
    }

    private static IReadOnlyList<T> FilterToWinners<T, TKey>(
        IReadOnlyList<T> rows,
        Func<T, TKey> groupKeySelector,
        Func<T, string?> fileNameSelector,
        Func<T, DateTime?> createdDateTimeSelector,
        Func<T, bool> isEligibleSelector)
        where TKey : notnull
    {
        var winningFileNameByGroup = WinningFileNames(
            rows, groupKeySelector, fileNameSelector, createdDateTimeSelector, isEligibleSelector);

        return rows
            .Where(r => winningFileNameByGroup.TryGetValue(groupKeySelector(r), out var winningFileName) &&
                        fileNameSelector(r) == winningFileName)
            .ToList();
    }
}
