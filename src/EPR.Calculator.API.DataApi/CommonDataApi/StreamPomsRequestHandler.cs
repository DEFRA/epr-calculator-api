using System.Diagnostics;
using System.Runtime.CompilerServices;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.Infrastructure;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi.CommonDataApi;

internal interface IStreamPomsRequestHandler
{
    IAsyncEnumerable<PayCalPom> Handle(int relativeYear, CancellationToken cancellationToken = default);
}

internal sealed class StreamPomsRequestHandler(HttpClient httpClient, IOptions<CommonDataApiHttpClientOptions> options)
    : IStreamPomsRequestHandler
{
    public async IAsyncEnumerable<PayCalPom> Handle(int relativeYear,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = DataApiTelemetry.StartActivity(typeof(StreamPomsRequestHandler), nameof(Handle));

        var poms = NdJsonHttpReader.ReadAsync<PayCalPom>(
            httpClient,
            $"api/paycal/v2/poms?RelativeYear={relativeYear}",
            options.Value.StreamStartTimeout,
            cancellationToken);

        await foreach (var pom in poms.WithCancellation(cancellationToken))
        {
            yield return pom;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
