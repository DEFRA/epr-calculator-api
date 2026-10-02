using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Api.DataApi.CommonDataApi;

/// <summary>
///     Configuration for the HTTP client that streams organisation/POM data from common-data-api's
///     v2 paycal endpoints.
/// </summary>
internal sealed record CommonDataApiHttpClientOptions
{
    public const string SectionKey = "CommonDataApi:HttpClient";

    /// <summary>Base URL of common-data-api.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; init; } = null!;

    /// <summary>Enables response compression for requests made by the client.</summary>
    public bool CompressionEnabled { get; init; } = true;

    /// <summary>Data streams that don't start within this time period will be cancelled.</summary>
    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan StreamStartTimeout { get; init; } = TimeSpan.FromMinutes(15);
}
