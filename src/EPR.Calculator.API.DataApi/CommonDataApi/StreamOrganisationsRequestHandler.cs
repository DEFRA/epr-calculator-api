using System.Diagnostics;
using System.Runtime.CompilerServices;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.Infrastructure;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi.CommonDataApi;

internal interface IStreamOrganisationsRequestHandler
{
    IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear, CancellationToken cancellationToken = default);
}

internal sealed class StreamOrganisationsRequestHandler(HttpClient httpClient, IOptions<CommonDataApiHttpClientOptions> options)
    : IStreamOrganisationsRequestHandler
{
    public async IAsyncEnumerable<PayCalOrganisation> Handle(int relativeYear,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(StreamOrganisationsRequestHandler), nameof(Handle));

        var organisations = NdJsonHttpReader.ReadAsync<PayCalOrganisation>(
            httpClient,
            $"api/paycal/v2/organisations?RelativeYear={relativeYear}",
            options.Value.StreamStartTimeout,
            cancellationToken);

        await foreach (var organisation in organisations.WithCancellation(cancellationToken))
        {
            yield return organisation;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
