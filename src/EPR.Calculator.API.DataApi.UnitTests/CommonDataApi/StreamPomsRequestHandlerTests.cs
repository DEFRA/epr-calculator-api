using System.Net;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi;

/// <summary>
///     Unit tests for <see cref="StreamPomsRequestHandler" />.
/// </summary>
[TestClass]
public class StreamPomsRequestHandlerTests
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
        capturedUrl.ShouldStartWith("/api/paycal/v2/poms?");
        capturedUrl.ShouldContain("RelativeYear=2025");
        capturedUrl.ToLowerInvariant().ShouldNotContain("cutoffdate");
    }

    [TestMethod]
    public async Task Handle_WithValidNdJson_DeserialisesPoms()
    {
        var ndJson = JsonSerializer.Serialize(new
        {
            organisationId = 100,
            submissionPeriod = "2024-H1",
            packagingType = "HH",
            packagingMaterial = "PL",
            packagingMaterialWeight = 123.4
        });
        var handler = CreateHandler(new MockHandler(_ => OkNdJson(ndJson)));

        var results = await CollectAsync(handler.Handle(2025));

        results.Count.ShouldBe(1);
        results[0].OrganisationId.ShouldBe(100);
        results[0].SubmissionPeriod.ShouldBe("2024-H1");
        results[0].PackagingType.ShouldBe("HH");
        results[0].PackagingMaterial.ShouldBe("PL");
        results[0].PackagingMaterialWeight.ShouldBe(123.4);
    }

    [TestMethod]
    public async Task Handle_WhenServerReturnsError_ThrowsHttpRequestException()
    {
        var handler = CreateHandler(new MockHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Should.ThrowAsync<HttpRequestException>(async () => await CollectAsync(handler.Handle(2025)));
    }

    private static StreamPomsRequestHandler CreateHandler(HttpMessageHandler messageHandler)
    {
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://test-api.example.com") };
        var options = new OptionsWrapper<CommonDataApiHttpClientOptions>(new CommonDataApiHttpClientOptions
        {
            BaseUrl = "https://test-api.example.com",
            StreamStartTimeout = TimeSpan.FromSeconds(30)
        });
        return new StreamPomsRequestHandler(httpClient, options);
    }

    private static HttpResponseMessage OkNdJson(string content) =>
        new(HttpStatusCode.OK) { Content = new StringContent(content, Encoding.UTF8, "application/x-ndjson") };

    private static async Task<List<PayCalPom>> CollectAsync(IAsyncEnumerable<PayCalPom> stream)
    {
        var list = new List<PayCalPom>();
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
