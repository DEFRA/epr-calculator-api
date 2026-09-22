using System.Net;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi;

/// <summary>
///     Unit tests for <see cref="StreamOrganisationsRequestHandler" />.
/// </summary>
[TestClass]
public class StreamOrganisationsRequestHandlerTests
{
    [TestMethod]
    public async Task Handle_RequestsV2EndpointWithRelativeYearOnly()
    {
        string? capturedUrl = null;
        var handler = CreateHandler(new MockHandler(url =>
        {
            capturedUrl = url;
            return OkNdJson(string.Empty);
        }));

        await CollectAsync(handler.Handle(2025));

        capturedUrl.ShouldNotBeNull();
        capturedUrl.ShouldStartWith("/api/paycal/v2/organisations?");
        capturedUrl.ShouldContain("RelativeYear=2025");
        capturedUrl.ToLowerInvariant().ShouldNotContain("cutoffdate");
    }

    [TestMethod]
    public async Task Handle_WithValidNdJson_DeserialisesOrganisations()
    {
        var ndJson = JsonSerializer.Serialize(new
        {
            organisationId = 100,
            organisationName = "Org Co",
            subsidiaryId = "S1",
            leaverCode = "02",
            regulatorStatus = "Granted",
            submissionPeriodYear = 2025
        });
        var handler = CreateHandler(new MockHandler(_ => OkNdJson(ndJson)));

        var results = await CollectAsync(handler.Handle(2025));

        results.Count.ShouldBe(1);
        results[0].OrganisationId.ShouldBe(100);
        results[0].OrganisationName.ShouldBe("Org Co");
        results[0].SubsidiaryId.ShouldBe("S1");
        results[0].StatusCode.ShouldBe("02");
        results[0].RegulatorStatus.ShouldBe("Granted");
    }

    [TestMethod]
    public async Task Handle_WhenServerReturnsError_ThrowsHttpRequestException()
    {
        var handler = CreateHandler(new MockHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Should.ThrowAsync<HttpRequestException>(async () => await CollectAsync(handler.Handle(2025)));
    }

    private static StreamOrganisationsRequestHandler CreateHandler(HttpMessageHandler messageHandler)
    {
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://test-api.example.com") };
        var options = new OptionsWrapper<CommonDataApiHttpClientOptions>(new CommonDataApiHttpClientOptions
        {
            BaseUrl = "https://test-api.example.com",
            StreamStartTimeout = TimeSpan.FromSeconds(30)
        });
        return new StreamOrganisationsRequestHandler(httpClient, options);
    }

    private static HttpResponseMessage OkNdJson(string content) =>
        new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/x-ndjson") };

    private static async Task<List<PayCalOrganisation>> CollectAsync(IAsyncEnumerable<PayCalOrganisation> stream)
    {
        var list = new List<PayCalOrganisation>();
        await foreach (var item in stream)
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>A mock HTTP message handler that delegates response creation to a factory function.</summary>
    private sealed class MockHandler(Func<string, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request.RequestUri?.PathAndQuery ?? string.Empty));
    }
}
