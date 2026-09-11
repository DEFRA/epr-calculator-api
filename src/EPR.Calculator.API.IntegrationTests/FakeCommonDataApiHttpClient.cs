using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.API.IntegrationTests;

public class FakeCommonDataApiClient : ICommonDataApiClient
{
    // A factory, invoked fresh on each StreamPoms() call, so POM rows are streamed off disk rather
    // than held in memory as a list - matching the real client, which reads the NDJSON response one
    // record at a time.
    public Func<IEnumerable<PomResponse>> Poms { get; set; } = () => [];

    public ImmutableList<OrganisationResponse> OrganisationResponses { get; set; } = [];

    public async IAsyncEnumerable<PomResponse> StreamPoms(
        RelativeYear relativeYear,
        DateTime? cutOffDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var pom in Poms())
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return pom;

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<OrganisationResponse> StreamOrganisations(
        RelativeYear relativeYear,
        DateTime? cutOffDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var organisation in OrganisationResponses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return organisation;

            await Task.Yield();
        }
    }
}
