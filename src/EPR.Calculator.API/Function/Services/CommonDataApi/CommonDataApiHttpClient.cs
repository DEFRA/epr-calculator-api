using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Options;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Services.CommonDataApi
{

    public interface ICommonDataApiClient
    {
        IAsyncEnumerable<PomResponse> StreamPoms(
            RelativeYear relativeYear,
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<OrganisationResponse> StreamOrganisations(
            RelativeYear relativeYear,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    ///     HTTP client for the Common Data API.
    /// </summary>
    public class CommonDataApiHttpClient : ICommonDataApiClient
    {
        private static readonly JsonSerializerOptions NdJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly CommonDataApiHttpClientOptions _opts;

        public CommonDataApiHttpClient(HttpClient httpClient, IOptions<CommonDataApiHttpClientOptions> options)
        {
            _httpClient = httpClient;
            _opts = options.Value;

            httpClient.BaseAddress = new Uri(_opts.BaseUrl);

            // Disable the built-in HttpClient timeout so that the per-request
            // StreamStartTimeout (via CancellationTokenSource) is the sole timeout.
            httpClient.Timeout = Timeout.InfiniteTimeSpan;

            if (_opts.CompressionEnabled)
            {
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }
        }

        /// <summary>
        ///     Streams organisation records from the Common Data API for the specified relative year.
        /// </summary>
        /// <param name="relativeYear">The relative year to query.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An async enumerable of <see cref="OrganisationResponse" /> records.</returns>
        public IAsyncEnumerable<OrganisationResponse> StreamOrganisations(RelativeYear relativeYear,
            CancellationToken cancellationToken = default)
        {
            if (relativeYear == null)
            {
                throw new ArgumentNullException(nameof(relativeYear));
            }

            var url = $"/api/paycal/organisations/stream?relativeYear={relativeYear}";
            return ReadNdJsonStreamAsync<OrganisationResponse>(url, cancellationToken);
        }

        /// <summary>
        ///     Streams POM records from the Common Data API for the specified relative year.
        /// </summary>
        /// <param name="relativeYear">The relative year to query.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An async enumerable of <see cref="PomResponse" /> records.</returns>
        public IAsyncEnumerable<PomResponse> StreamPoms(RelativeYear relativeYear,
            CancellationToken cancellationToken = default)
        {
            if (relativeYear == null)
            {
                throw new ArgumentNullException(nameof(relativeYear));
            }

            var url = $"/api/paycal/poms/stream?relativeYear={relativeYear}";
            return ReadNdJsonStreamAsync<PomResponse>(url, cancellationToken);
        }

        private async IAsyncEnumerable<T> ReadNdJsonStreamAsync<T>(
            string requestUri,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Timeout to cancel request if it takes too long for the stream to start
            using var timeoutCts = new CancellationTokenSource(_opts.StreamStartTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Force HTTP/1.1 for requests. HTTP/2's framing layer can buffer
            // response data in a way that prevents line-by-line NDJSON streaming
            // from working correctly (notably on macOS).
            request.Version = HttpVersion.Version11;

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
            response.EnsureSuccessStatusCode();

            var stream = await GetResponseStream(response, linkedCts.Token);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var record = JsonSerializer.Deserialize<T>(line, NdJsonOptions);

                if (record is not null)
                {
                    yield return record;
                }
            }
        }

        private static async Task<Stream> GetResponseStream(HttpResponseMessage response, CancellationToken ct)
        {
            var stream = await response.Content.ReadAsStreamAsync(ct);
            var contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

            if (contentEncoding != null)
            {
                stream = contentEncoding.ToLowerInvariant() switch
                {
                    "gzip" => new GZipStream(stream, CompressionMode.Decompress),
                    "deflate" => new DeflateStream(stream, CompressionMode.Decompress),
                    _ => stream
                };
            }

            return stream;
        }
    }
}