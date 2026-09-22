using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace EPR.Calculator.Api.DataApi.CommonDataApi.Infrastructure;

/// <summary>
///     Streams an NDJSON (newline-delimited JSON) HTTP response one record at a time, shared by
///     the organisation/POM stream request handlers so neither buffers the whole response in memory.
/// </summary>
internal static class NdJsonHttpReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async IAsyncEnumerable<T> ReadAsync<T>(
        HttpClient httpClient,
        string requestUri,
        TimeSpan streamStartTimeout,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Timeout to cancel the request if it takes too long for the stream to start.
        using var timeoutCts = new CancellationTokenSource(streamStartTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri)
        {
            // Force HTTP/1.1. HTTP/2's framing layer can buffer response data in a way that
            // prevents incremental NDJSON streaming from working correctly (notably on macOS).
            // VersionPolicy is required too - Version alone only sets a preference, and the
            // default policy (RequestVersionOrHigher) lets the connection upgrade to HTTP/2 over
            // TLS via ALPN whenever the server offers it, silently defeating the Version setting.
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);

        // Tagged on the caller's activity (StreamOrganisationsRequestHandler/StreamPomsRequestHandler
        // each start one) before EnsureSuccessStatusCode can throw, so a failed call is still visible.
        Activity.Current?.SetTag("http_request_url", response.RequestMessage?.RequestUri?.ToString());
        Activity.Current?.SetTag("http_status_code", (int)response.StatusCode);

        response.EnsureSuccessStatusCode();

        await using var stream = await GetResponseStream(response, linkedCts.Token);

        // topLevelValues: true reads whitespace-separated top-level JSON values, which is exactly
        // the NDJSON wire format, straight from the UTF-8 bytes.
        var records = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, topLevelValues: true, Options, cancellationToken);

        await foreach (var record in records)
        {
            if (record is not null)
            {
                yield return record;
            }
        }
    }

    private static async Task<Stream> GetResponseStream(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

        return contentEncoding?.ToLowerInvariant() switch
        {
            "gzip" => new GZipStream(stream, CompressionMode.Decompress),
            "deflate" => new DeflateStream(stream, CompressionMode.Decompress),
            _ => stream
        };
    }
}
