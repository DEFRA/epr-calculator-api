using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi;

namespace EPR.Calculator.API.BackgroundService.Services.DataLoading;

/// <summary>
///     Loads producer data required for a calculator run.
/// </summary>
public interface IDataLoader
{
    /// <summary>
    ///     Loads data for the specified calculator run: a single call into DataApi that streams and
    ///     fully processes organisation/POM data into producer records ready for calculation, including
    ///     any errors/warnings raised along the way.
    /// </summary>
    Task<IReadOnlyList<ProducerRecord>> LoadData(CalculatorRunContext runContext, CancellationToken cancellationToken = default);
}

/// <summary>
///     Loads producer data by making a single request to DataApi. Performs no persistence - that's
///     the caller's responsibility. Whether DataApi stages the source through the load tables first is
///     decided inside DataApi from CommonDataApi:DataLoader:Enabled.
/// </summary>
public class CommonDataApiLoader(
    IProducerDataService producerDataService,
    IMaterialService materialService,
    ILogger<CommonDataApiLoader> logger
) : IDataLoader
{
    /// <inheritdoc />
    public Task<IReadOnlyList<ProducerRecord>> LoadData(
        CalculatorRunContext runContext, CancellationToken cancellationToken = default) =>
        LoadDataCore(runContext, cancellationToken);

    [ActivityTrace]
    private async Task<IReadOnlyList<ProducerRecord>> LoadDataCore(RunContext runContext, CancellationToken cancellationToken)
    {
        var cutOffDate = runContext.DefaultParameters.CutOffDate is { } d
            ? new DateTimeOffset(DateTime.SpecifyKind(d, DateTimeKind.Utc))
            : (DateTimeOffset?)null;

        var materials = await materialService.GetMaterials();
        var materialCodes = materials.Select(m => m.Code).ToImmutableList();

        var data = await producerDataService.GetProducerData(
            runContext.RelativeYear,
            cutOffDate,
            materialCodes,
            cancellationToken);

        logger.LogTrace("Loaded {TotalProducers} producer records", data.Count);

        return data;
    }
}
