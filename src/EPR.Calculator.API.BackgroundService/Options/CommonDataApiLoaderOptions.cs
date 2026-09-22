using EPR.Calculator.API.BackgroundService.Services.DataLoading;

namespace EPR.Calculator.API.BackgroundService.Options;

/// <summary>
///     Configuration options for <see cref="CommonDataApiLoader" />.
/// </summary>
public record CommonDataApiLoaderOptions
{
    public const string SectionKey = "CommonDataApi:DataLoader";

    public bool Enabled { get; init; } = true;
}
