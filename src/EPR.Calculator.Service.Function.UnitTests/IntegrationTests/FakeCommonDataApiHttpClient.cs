using System.Runtime.CompilerServices;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Services.CommonDataApi;

namespace EPR.Calculator.Service.Function.UnitTests.IntegrationTests;

public class FakeCommonDataApiClient : ICommonDataApiClient
{
    public ImmutableList<PomResponse> PomResponses { get; set; } = [];
    public ImmutableList<OrganisationResponse> OrganisationResponses { get; set; } = [];

    public async IAsyncEnumerable<PomResponse> StreamPoms(
        RelativeYear relativeYear,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var pom in PomResponses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return pom;

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<OrganisationResponse> StreamOrganisations(
        RelativeYear relativeYear,
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