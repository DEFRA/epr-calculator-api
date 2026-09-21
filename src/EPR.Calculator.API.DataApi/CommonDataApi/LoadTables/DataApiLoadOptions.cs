namespace EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;

/// <summary>
///     Configuration for the DataApi load-table stage, bound from <c>CommonDataApi:DataLoader</c>.
/// </summary>
internal sealed record DataApiLoadOptions
{
    public const string SectionKey = "CommonDataApi:DataLoader";

    /// <summary>
    ///     <c>true</c>: stream the RPD source once into <c>data_api_load_*</c>, then run the pipeline off
    ///     those tables. <c>false</c>: read the RPD source directly (the two dedup passes query it twice).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Rows per <c>BulkInsert</c> batch when filling <c>data_api_load_poms</c>.</summary>
    public int PomBatchSize { get; init; } = 10_000;

    /// <summary>Rows per <c>BulkInsert</c> batch when filling <c>data_api_load_organisations</c>.</summary>
    public int OrganisationBatchSize { get; init; } = 10_000;
}
