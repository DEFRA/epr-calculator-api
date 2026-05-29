using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Service.Function.Services.CommonDataApi;

namespace EPR.Calculator.Service.Function.Options;

/// <summary>
///     Configuration options for <see cref="CommonDataApiHttpClient" />.
/// </summary>
public record CommonDataApiHttpClientOptions
{
    public const string SectionKey = "CommonDataApi:HttpClient";

    /// <summary>
    ///     Base URL of the Common Data API.
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; init; } = null!;

    /// <summary>
    ///     Enables response compression for requests made by the client.
    /// </summary>
    public bool CompressionEnabled { get; init; } = true;

    /// <summary>
    ///     Data streams that don't start within this time period will be cancelled.
    /// </summary>
    [Required]
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan StreamStartTimeout { get; init; } = TimeSpan.FromMinutes(15);
}
