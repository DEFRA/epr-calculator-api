using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.Api.DataApi.Models;
using EPR.Calculator.Api.DataApi.Services;

namespace EPR.Calculator.API.BackgroundService.Services;

/// <summary>
///     Loads producer data required for a calculator run.
/// </summary>
public interface ICalculatorDataApiService
{
    /// <summary>
    ///     Loads data for the specified calculator run: a single call into DataApi that fully processes
    ///     organisation/POM data into producer records ready for calculation, including any
    ///     errors/warnings raised along the way.
    /// </summary>
    Task<IReadOnlyList<ProducerRecord>> GetProducerRecords(CalculatorRunContext runContext, CancellationToken cancellationToken = default);
}

/// <summary>
///     Loads producer data by making a single request to DataApi. Performs no persistence - that's
///     the caller's responsibility.
/// </summary>
public class CalculatorDataApiService(
    IProducerDataService producerDataService,
    IMaterialService materialService,
    ILogger<CalculatorDataApiService> logger
) : ICalculatorDataApiService
{
    /// <inheritdoc />
    [ActivityTrace]
    public async Task<IReadOnlyList<ProducerRecord>> GetProducerRecords(
        CalculatorRunContext runContext, CancellationToken cancellationToken = default)
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
