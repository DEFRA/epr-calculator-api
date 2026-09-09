using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.AcceptedFileSelection;

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
/// </summary>
public interface IAcceptedFileSelector
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

public sealed class AcceptedFileSelector : IAcceptedFileSelector
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
                p => (p.OrganisationId, p.SubmitterId, p.SubmissionPeriod),
                p => p.FileName,
                p => p.IsResubmission,
                p => p.CreatedDateTime,
                cutOffDate));

    public IReadOnlyDictionary<(int? OrganisationId, string? SubmitterId, string? SubmissionPeriod), string?>
        SelectWinningPomFileNames(IEnumerable<PomFileCandidate> candidates, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectWinningPomFileNames),
            () => WinningFileNames(
                candidates,
                c => (c.OrganisationId, c.SubmitterId, c.SubmissionPeriod),
                c => c.FileName,
                c => c.IsResubmission,
                c => c.CreatedDateTime,
                cutOffDate));

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
