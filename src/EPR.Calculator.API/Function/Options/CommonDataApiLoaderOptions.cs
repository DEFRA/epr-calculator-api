using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.Options;

/// <summary>
///     Configuration options for <see cref="CommonDataApiLoader" />.
/// </summary>
public record CommonDataApiLoaderOptions
{
    public const string SectionKey = "CommonDataApi:DataLoader";

    public bool Enabled { get; init; } = true;

    /// <summary>
    ///     Number of POM records to stream into memory before bulk inserting to the database.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PomBatchSize { get; init; } = 10000;

    /// <summary>
    ///     Number of Organisation records to stream into memory before bulk inserting to the database.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int OrganisationBatchSize { get; init; } = 10000;
}
