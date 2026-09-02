using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.ObligationDetermination;

namespace EPR.Calculator.API.IntegrationTests;

public class FakeStreamOrganisationsRequestHandler : IStreamOrganisationsRequestHandler
{
    public ImmutableList<PayCalOrganisation> Organisations { get; set; } = [];

    public async IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, DateTimeOffset? cutOffDate)
    {
        foreach (var organisation in Organisations)
        {
            yield return organisation;

            await Task.Yield();
        }
    }
}

public class FakeStreamPomsRequestHandler : IStreamPomsRequestHandler
{
    public ImmutableList<PayCalPom> Poms { get; set; } = [];

    public async IAsyncEnumerable<PayCalPom> Handle(int relativeYear, DateTimeOffset? cutOffDate)
    {
        foreach (var pom in Poms)
        {
            yield return pom;

            await Task.Yield();
        }
    }
}

/// <summary>
///     Test fixtures seed <see cref="PayCalOrganisation" /> rows with ObligationStatus/ErrorCode/
///     NumDaysObligated already resolved (a snapshot of already-determined data, not the raw multi-row
///     registrations a real determiner needs), so the real <see cref="IProducerObligationDeterminer" />
///     is bypassed in favour of this pass-through in integration tests.
/// </summary>
public class PassthroughProducerObligationDeterminer : IProducerObligationDeterminer
{
    public IReadOnlyList<PayCalOrganisation> Determine(IReadOnlyList<PayCalOrganisation> organisations) => organisations;
}
