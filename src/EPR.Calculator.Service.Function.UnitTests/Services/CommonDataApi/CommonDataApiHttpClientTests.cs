using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.UnitTests.Services.CommonDataApi
{
    /// <summary>
    ///     Unit tests for <see cref="CommonDataApiHttpClient" />.
    /// </summary>
    [TestClass]
    public class CommonDataApiHttpClientTests
    {
        private static readonly string ValidGuid = "11111111-1111-1111-1111-111111111111";

        private readonly CommonDataApiHttpClientOptions _apiHttpClientOptions = new()
        {
            BaseUrl = "https://test-api.example.com"
        };

        /// <summary>
        ///     Verifies that valid POM NDJSON is deserialized into the correct number of records.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WithValidNdJson_DeserializesAllRecords()
        {
            // Arrange
            var handler = CreateOkHandler(CreatePomNdJson(3));
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(1, results[0].OrganisationId);
            Assert.AreEqual(2, results[1].OrganisationId);
            Assert.AreEqual(3, results[2].OrganisationId);
        }

        /// <summary>
        ///     Verifies that valid Organisation NDJSON is deserialized into the correct number of records.
        /// </summary>
        [TestMethod]
        public async Task StreamOrganisations_WithValidNdJson_DeserializesAllRecords()
        {
            // Arrange
            var handler = CreateOkHandler(CreateOrgNdJson(2));
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamOrganisations(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("Organisation 1", results[0].OrganisationName);
            Assert.AreEqual("Organisation 2", results[1].OrganisationName);
        }

        /// <summary>
        ///     Verifies that a server error on the POM stream throws <see cref="HttpRequestException" />.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WhenServerReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = CreateErrorHandler(HttpStatusCode.InternalServerError);
            var client = CreateClient(handler);

            // Act & Assert
            await Should.ThrowAsync<HttpRequestException>(
                async () => await CollectAsync(client.StreamPoms(new RelativeYear(2024))));
        }

        /// <summary>
        ///     Verifies that a server error on the Organisation stream throws <see cref="HttpRequestException" />.
        /// </summary>
        [TestMethod]
        public async Task StreamOrganisations_WhenServerReturnsError_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = CreateErrorHandler(HttpStatusCode.InternalServerError);
            var client = CreateClient(handler);

            // Act & Assert
            await Should.ThrowAsync<HttpRequestException>(
                async () => await CollectAsync(client.StreamOrganisations(new RelativeYear(2024))));
        }

        /// <summary>
        ///     Verifies that the relative year is substituted into the POM URL.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_FormatsUrlWithRelativeYear()
        {
            // Arrange
            string? capturedUrl = null;
            var handler = new MockHandler(url =>
            {
                capturedUrl = url;
                return OkNdJson(string.Empty);
            });
            var client = CreateClient(handler);

            // Act
            await CollectAsync(client.StreamPoms(new RelativeYear(2025)));

            // Assert
            Assert.IsNotNull(capturedUrl);
            StringAssert.Contains(capturedUrl, "relativeYear=2025");
        }

        /// <summary>
        ///     Verifies that the relative year is substituted into the Organisation URL.
        /// </summary>
        [TestMethod]
        public async Task StreamOrganisations_FormatsUrlWithRelativeYear()
        {
            // Arrange
            string? capturedUrl = null;
            var handler = new MockHandler(url =>
            {
                capturedUrl = url;
                return OkNdJson(string.Empty);
            });
            var client = CreateClient(handler);

            // Act
            await CollectAsync(client.StreamOrganisations(new RelativeYear(2025)));

            // Assert
            Assert.IsNotNull(capturedUrl);
            StringAssert.Contains(capturedUrl, "relativeYear=2025");
        }

        /// <summary>
        ///     Verifies that blank lines in the NDJSON stream are skipped.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_SkipsBlankLines()
        {
            // Arrange
            var pom = new PomResponse { OrganisationId = 1, SubmitterId = ValidGuid };
            var ndJson = $"\n{JsonSerializer.Serialize(pom)}\n\n";
            var handler = CreateOkHandler(ndJson);
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(1, results.Count);
        }

        /// <summary>
        ///     Verifies that StreamPoms throws ArgumentNullException when relativeYear is null.
        /// </summary>
        [TestMethod]
        public void StreamPoms_WithNullRelativeYear_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = CreateOkHandler(string.Empty);
            var client = CreateClient(handler);

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => client.StreamPoms(null!));
        }

        /// <summary>
        ///     Verifies that StreamOrganisations throws ArgumentNullException when relativeYear is null.
        /// </summary>
        [TestMethod]
        public void StreamOrganisations_WithNullRelativeYear_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = CreateOkHandler(string.Empty);
            var client = CreateClient(handler);

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => client.StreamOrganisations(null!));
        }

        /// <summary>
        ///     Verifies that an empty response returns no records.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WithEmptyResponse_ReturnsNoRecords()
        {
            // Arrange
            var handler = CreateOkHandler(string.Empty);
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(0, results.Count);
        }

        private CommonDataApiHttpClient CreateClient(HttpMessageHandler handler) =>
            CreateClient(handler, _apiHttpClientOptions);

        private static CommonDataApiHttpClient CreateClient(HttpMessageHandler handler, CommonDataApiHttpClientOptions options)
        {
            var httpClient = new HttpClient(handler);
            return new CommonDataApiHttpClient(httpClient, new OptionsWrapper<CommonDataApiHttpClientOptions>(options));
        }

        private static MockHandler CreateOkHandler(string ndJsonContent) =>
            new(_ => OkNdJson(ndJsonContent));

        private static MockHandler CreateErrorHandler(HttpStatusCode statusCode) =>
            new(_ => new HttpResponseMessage(statusCode));

        private static HttpResponseMessage OkNdJson(string content) =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/x-ndjson")
            };

        private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> stream)
        {
            var list = new List<T>();
            await foreach (var item in stream)
            {
                list.Add(item);
            }

            return list;
        }

        private static string CreatePomNdJson(int count)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                var pom = new PomResponse
                {
                    OrganisationId = i + 1,
                    SubmissionPeriod = $"2024-Q{i % 4 + 1}",
                    PackagingType = "HH",
                    PackagingMaterial = "PL",
                    PackagingMaterialWeight = 100.0 + i,
                    SubmitterId = ValidGuid
                };
                sb.AppendLine(JsonSerializer.Serialize(pom));
            }

            return sb.ToString();
        }

        private static string CreateOrgNdJson(int count)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                var org = new OrganisationResponse
                {
                    OrganisationId = i + 1,
                    OrganisationName = $"Organisation {i + 1}",
                    ObligationStatus = "Full",
                    SubmitterId = ValidGuid
                };
                sb.AppendLine(JsonSerializer.Serialize(org));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Verifies that Accept-Encoding headers are added when compression is enabled.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WhenCompressionEnabled_SendsAcceptEncodingHeaders()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = new RequestCapturingHandler(request =>
            {
                capturedRequest = request;
                return OkNdJson(string.Empty);
            });
            var options = new CommonDataApiHttpClientOptions
            {
                BaseUrl = "https://test-api.example.com",
                CompressionEnabled = true
            };
            var client = CreateClient(handler, options);

            // Act
            await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.IsNotNull(capturedRequest);
            var encodings = capturedRequest.Headers.AcceptEncoding.Select(e => e.Value).ToList();
            CollectionAssert.Contains(encodings, "gzip");
            CollectionAssert.Contains(encodings, "deflate");
        }

        /// <summary>
        ///     Verifies that Accept-Encoding headers are not added when compression is disabled.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WhenCompressionDisabled_DoesNotSendAcceptEncodingHeaders()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = new RequestCapturingHandler(request =>
            {
                capturedRequest = request;
                return OkNdJson(string.Empty);
            });
            var options = new CommonDataApiHttpClientOptions
            {
                BaseUrl = "https://test-api.example.com",
                CompressionEnabled = false
            };
            var client = CreateClient(handler, options);

            // Act
            await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.IsNotNull(capturedRequest);
            Assert.AreEqual(0, capturedRequest.Headers.AcceptEncoding.Count);
        }

        /// <summary>
        ///     Verifies that a gzip-compressed response is decompressed and deserialized correctly.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WithGzipResponse_DecompressesCorrectly()
        {
            // Arrange
            var handler = CreateCompressedHandler(CreatePomNdJson(2), "gzip", GzipCompress);
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, results[0].OrganisationId);
            Assert.AreEqual(2, results[1].OrganisationId);
        }

        /// <summary>
        ///     Verifies that a deflate-compressed response is decompressed and deserialized correctly.
        /// </summary>
        [TestMethod]
        public async Task StreamPoms_WithDeflateResponse_DecompressesCorrectly()
        {
            // Arrange
            var handler = CreateCompressedHandler(CreatePomNdJson(2), "deflate", DeflateCompress);
            var client = CreateClient(handler);

            // Act
            var results = await CollectAsync(client.StreamPoms(new RelativeYear(2024)));

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, results[0].OrganisationId);
            Assert.AreEqual(2, results[1].OrganisationId);
        }

        private static MockHandler CreateCompressedHandler(string ndJsonContent, string encoding, Func<byte[], byte[]> compress) =>
            new(_ =>
            {
                var compressed = compress(Encoding.UTF8.GetBytes(ndJsonContent));
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(compressed)
                };
                response.Content.Headers.ContentEncoding.Add(encoding);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-ndjson");
                return response;
            });

        private static byte[] GzipCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        private static byte[] DeflateCompress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var deflate = new DeflateStream(output, CompressionMode.Compress))
            {
                deflate.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }

        /// <summary>
        ///     A mock HTTP message handler that delegates response creation to a factory function.
        /// </summary>
        private sealed class MockHandler(Func<string, HttpResponseMessage> responseFactory) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var url = request.RequestUri?.PathAndQuery ?? string.Empty;
                return Task.FromResult(responseFactory(url));
            }
        }

        /// <summary>
        ///     A mock HTTP message handler that exposes the full request to the factory function.
        /// </summary>
        private sealed class RequestCapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(responseFactory(request));
            }
        }
    }
}
