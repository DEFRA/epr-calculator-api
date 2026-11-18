namespace EPR.Calculator.API.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record FeatureFlagOptions
{
    public const string SectionKey = "FeatureFlags";

    /// <summary>
    ///     Whether the results/billing file download endpoints serve the files the runs wrote to blob storage, rather than
    ///     building them from the database.
    /// </summary>
    public bool DownloadFromBlobStorage { get; init; }

    /// <summary>
    ///     Whether runs can be imported from their billing CSVs.
    /// </summary>
    public bool RunImport { get; init; }
}
