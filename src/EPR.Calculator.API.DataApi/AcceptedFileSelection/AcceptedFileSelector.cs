using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

namespace EPR.CommonDataService.DataApi.AcceptedFileSelection;

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
}

public sealed class AcceptedFileSelector : IAcceptedFileSelector
{
    public IReadOnlyList<PayCalOrganisation> SelectLatestOrganisationFiles(
        IReadOnlyList<PayCalOrganisation> organisations, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestOrganisationFiles),
            () => SelectLatestFiles(
                organisations,
                o => (o.OrganisationId, o.SubmitterId, o.SubmissionPeriodYear),
                o => o.FileName,
                o => o.IsResubmission,
                o => o.CreatedDateTime,
                cutOffDate));

    public IReadOnlyList<PayCalPom> SelectLatestPomFiles(
        IReadOnlyList<PayCalPom> poms, DateTimeOffset? cutOffDate) =>
        DataApiTelemetry.Trace(typeof(AcceptedFileSelector), nameof(SelectLatestPomFiles),
            () => SelectLatestFiles(
                poms,
                p => (p.OrganisationId, p.SubmitterId, p.SubmissionPeriod),
                p => p.FileName,
                p => p.IsResubmission,
                p => p.CreatedDateTime,
                cutOffDate));

    private static IReadOnlyList<T> SelectLatestFiles<T, TKey>(
        IReadOnlyList<T> rows,
        Func<T, TKey> groupKeySelector,
        Func<T, string?> fileNameSelector,
        Func<T, bool> isResubmissionSelector,
        Func<T, DateTime?> createdDateTimeSelector,
        DateTimeOffset? cutOffDate)
        where TKey : notnull
    {
        var cutOff = cutOffDate?.UtcDateTime;

        var winningFileNameByGroup = rows
            .GroupBy(groupKeySelector)
            .Select(group => (
                group.Key,
                Winner: group
                    .Where(r => !isResubmissionSelector(r) || cutOff is null || createdDateTimeSelector(r) <= cutOff)
                    .MaxBy(createdDateTimeSelector)))
            .Where(group => group.Winner is not null)
            .ToDictionary(group => group.Key, group => fileNameSelector(group.Winner!));

        return rows
            .Where(r => winningFileNameByGroup.TryGetValue(groupKeySelector(r), out var winningFileName) &&
                        fileNameSelector(r) == winningFileName)
            .ToList();
    }
}
