using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi;
using Microsoft.Extensions.Options;

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
///     the caller's responsibility.
/// </summary>
public class CommonDataApiLoader(
    IOptions<CommonDataApiLoaderOptions> options,
    IProducerDataService producerDataService,
    IMaterialService materialService,
    ILogger<CommonDataApiLoader> logger
) : IDataLoader
{
    private static readonly IReadOnlyList<ProducerRecord> Empty = [];

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProducerRecord>> LoadData(
        CalculatorRunContext runContext, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Disabled, skipping load");
            return Empty;
        }

        return await LoadDataCore(runContext, cancellationToken);
    }

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
