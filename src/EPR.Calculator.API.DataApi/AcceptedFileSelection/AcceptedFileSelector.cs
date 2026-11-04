using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.PomEligibility;

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
    DateTime? CreatedDateTime);

/// <summary>
///     Picks the winning accepted file per organisation/submitter/period from the candidate rows streamed
///     by StreamOrganisationsRequestHandler/StreamPomsRequestHandler (which now return every accepted-status
///     candidate file, unfiltered), ported from the cut-off-date logic that previously lived inline in
///     those handlers' SQL. Within each group, the latest file wins unless it is a resubmission created
///     after <paramref name="cutOffDate" /> - in which case the search falls back to the latest file that
///     is still eligible (an original, or a resubmission created on/before the cut-off). Groups with no
///     eligible candidate are excluded entirely.
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
        IReadOnlyList<PayCalOrganisation> organisations, DateTimeOffset? cutOffDate);

    IReadOnlyList<PayCalPom> SelectLatestPomFiles(
        IReadOnlyList<PayCalPom> poms, DateTimeOffset? cutOffDate);

    /// <summary>
    ///     The winning file name per (organisation, submitter, period), decided from candidate file
    ///     metadata alone. Lets a caller stream the POM rows in two passes - one to gather the
    ///     candidates, one to keep only the winners' rows - instead of buffering every row.
    /// </summary>
    IReadOnlyDictionary<(int? OrganisationId, string? SubmitterId, string? SubmissionPeriod), string?>
        SelectWinningPomFileNames(IEnumerable<PomFileCandidate> candidates, DateTimeOffset? cutOffDate);
}

internal sealed class AcceptedFileSelector : IAcceptedFileSelector
{
    public IReadOnlyList<PayCalOrganisation> SelectLatestOrganisationFiles(
        IReadOnlyList<PayCalOrganisation> organisations, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestOrganisationFiles),
            () => FilterToWinners(
                organisations,
                o => (o.OrganisationId, o.SubmitterId, o.SubmissionPeriodYear),
                o => o.FileName,
                o => o.IsResubmission,
                o => o.CreatedDateTime,
                cutOffDate));

    public IReadOnlyList<PayCalPom> SelectLatestPomFiles(
        IReadOnlyList<PayCalPom> poms, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestPomFiles),
            () => FilterToWinners(
                poms,
                p => (p.OrganisationId, p.SubmitterId, HalfPeriod: ToH1OrH2(p.SubmissionPeriod)),
                p => p.FileName,
                p => p.IsResubmission,
                p => p.CreatedDateTime,
                cutOffDate));

    public IReadOnlyDictionary<(int? OrganisationId, string? SubmitterId, string? SubmissionPeriod), string?>
        SelectWinningPomFileNames(IEnumerable<PomFileCandidate> candidates, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectWinningPomFileNames),
            () =>
            {
                var candidateList = candidates as IReadOnlyList<PomFileCandidate> ?? candidates.ToList();

                var winnerByHalf = WinningFileNames(
                    candidateList,
                    c => (c.OrganisationId, c.SubmitterId, HalfPeriod: ToH1OrH2(c.SubmissionPeriod)),
                    c => c.FileName,
                    c => c.IsResubmission,
                    c => c.CreatedDateTime,
                    cutOffDate);

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
        Func<T, bool> isResubmissionSelector,
        Func<T, DateTime?> createdDateTimeSelector,
        DateTimeOffset? cutOffDate)
        where TKey : notnull
    {
        var cutOff = cutOffDate?.UtcDateTime;

        return rows
            .GroupBy(groupKeySelector)
            .Select(group => (
                group.Key,
                Winner: group
                    .Where(r => !isResubmissionSelector(r) || cutOff is null || createdDateTimeSelector(r) <= cutOff)
                    .MaxBy(createdDateTimeSelector)))
            .Where(group => group.Winner is not null)
            .ToDictionary(group => group.Key, group => fileNameSelector(group.Winner!));
    }

    private static IReadOnlyList<T> FilterToWinners<T, TKey>(
        IReadOnlyList<T> rows,
        Func<T, TKey> groupKeySelector,
        Func<T, string?> fileNameSelector,
        Func<T, bool> isResubmissionSelector,
        Func<T, DateTime?> createdDateTimeSelector,
        DateTimeOffset? cutOffDate)
        where TKey : notnull
    {
        var winningFileNameByGroup = WinningFileNames(
            rows, groupKeySelector, fileNameSelector, isResubmissionSelector, createdDateTimeSelector, cutOffDate);

        return rows
            .Where(r => winningFileNameByGroup.TryGetValue(groupKeySelector(r), out var winningFileName) &&
                        fileNameSelector(r) == winningFileName)
            .ToList();
    }
}
