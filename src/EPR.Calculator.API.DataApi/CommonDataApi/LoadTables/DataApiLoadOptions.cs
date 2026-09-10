namespace EPR.CommonDataService.DataApi.CommonDataApi.LoadTables;

/// <summary>
///     Configuration for the DataApi load-table stage, bound from <c>CommonDataApi:DataLoader</c>.
/// </summary>
public sealed record DataApiLoadOptions
{
    public const string SectionKey = "CommonDataApi:DataLoader";

    /// <summary>
    ///     <c>true</c>: stream the RPD source once into <c>data_api_load_*</c>, then run the pipeline off
    ///     those tables. <c>false</c>: read the RPD source directly (the two dedup passes query it twice).
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Rows per <c>BulkInsert</c> batch when filling the load tables.</summary>
    public int BatchSize { get; init; } = 10_000;
}
