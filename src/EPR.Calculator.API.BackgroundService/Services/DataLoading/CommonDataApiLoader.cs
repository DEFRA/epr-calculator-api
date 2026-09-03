using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Options;
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
    ///     fully processes organisation/POM data into producers ready for calculation, plus any
    ///     errors/warnings raised along the way.
    /// </summary>
    Task<ProducerCalculationData> LoadData(CalculatorRunContext runContext, CancellationToken cancellationToken = default);
}

/// <summary>
///     Loads producer data by making a single request to DataApi. Performs no persistence - that's
///     the caller's responsibility.
/// </summary>
public class CommonDataApiLoader(
    IOptions<CommonDataApiLoaderOptions> options,
    IProducerDataService producerDataService,
    IMaterialService materialService,
    IInvoicedProducerService invoicedProducerService,
    ILogger<CommonDataApiLoader> logger
) : IDataLoader
{
    private static readonly ProducerCalculationData Empty = new() { Organisations = [], Producers = [], Errors = [] };

    /// <inheritdoc />
    public async Task<ProducerCalculationData> LoadData(
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
    private async Task<ProducerCalculationData> LoadDataCore(RunContext runContext, CancellationToken cancellationToken)
    {
        var cutOffDate = runContext.DefaultParameters.CutOffDate is { } d
            ? new DateTimeOffset(DateTime.SpecifyKind(d, DateTimeKind.Utc))
            : (DateTimeOffset?)null;

        var materials = await materialService.GetMaterials();
        var materialCodes = materials.Select(m => m.Code).ToImmutableList();

        var invoicedProducers = await invoicedProducerService.GetInvoicedProducers(runContext.RelativeYear, cancellationToken: cancellationToken);
        var invoicedOrganisationIds = invoicedProducers.Select(i => i.ProducerId).ToHashSet();

        var data = await producerDataService.GetProducerData(
            runContext.RelativeYear,
            cutOffDate,
            materialCodes,
            invoicedOrganisationIds,
            cancellationToken);

        logger.LogTrace(
            "Loaded {TotalOrgs} organisations, {TotalProducers} producers and {TotalErrors} errors",
            data.Organisations.Count, data.Producers.Count, data.Errors.Count);

        return data;
    }
}
