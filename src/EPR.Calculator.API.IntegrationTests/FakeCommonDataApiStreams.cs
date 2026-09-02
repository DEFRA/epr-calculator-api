using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;

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
