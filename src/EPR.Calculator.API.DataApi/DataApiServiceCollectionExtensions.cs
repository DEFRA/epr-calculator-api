using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Infrastructure;
using EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;
using EPR.Calculator.Api.DataApi.ObligationDetermination;
using EPR.Calculator.Api.DataApi.PomEligibility;
using EPR.Calculator.Api.DataApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Api.DataApi;

public static class DataApiServiceCollectionExtensions
{
    /// <summary>
    ///     Registers DataApi. The only type callers resolve is <see cref="IProducerDataService" />; everything
    ///     else is an internal detail, so this is the single point where the host wires DataApi up.
    /// </summary>
    /// <param name="synapseConnectionString">Resolves the Synapse connection string when the context is first created.</param>
    /// <param name="loadTableConnectionString">Resolves the connection string of the database that hosts the data_api_load_* tables.</param>
    /// <param name="loadTablesEnabled">
    ///     Whether a run stages the RPD source through the load tables (true) or reads it directly (false).
    ///     Resolved when the options are first read, so the choice is made at run time.
    /// </param>
    /// <param name="captureMemoryMetrics">
    ///     Whether DataApi's activity traces carry allocated/heap-bytes tags - off by default, since
    ///     that was measured expensive under Azure Monitor at production activity volumes; the
    ///     performance test turns it on. Known at startup, so this is a plain value, not a factory.
    /// </param>
    public static IServiceCollection AddDataApi(
        this IServiceCollection services,
        Func<IServiceProvider, string> synapseConnectionString,
        Func<IServiceProvider, string> loadTableConnectionString,
        Func<IServiceProvider, bool> loadTablesEnabled,
        bool captureMemoryMetrics = false)
    {
        DataApiTelemetry.CaptureMemoryMetrics = captureMemoryMetrics;

        // Factory, not a scoped context: the org and POM streams query concurrently (ProducerDataService).
        services.AddDbContextFactory<SynapseContext>((provider, builder) =>
            builder
                .UseSqlServer(synapseConnectionString(provider))
                .AddInterceptors(new TimeoutInterceptor()));

        services.AddTransient<IStreamOrganisationsRequestHandler, StreamOrganisationsRequestHandler>();
        services.AddTransient<IStreamPomsRequestHandler, StreamPomsRequestHandler>();
        services.AddTransient<IAcceptedFileSelector, AcceptedFileSelector>();
        services.AddTransient<IProducerPomAligner, ProducerPomAligner>();
        services.AddTransient<IProducerObligationDeterminer, ProducerObligationDeterminer>();
        services.AddTransient<IPomEligibilityFilter, PomEligibilityFilter>();
        services.AddTransient<IOrganisationPeriodFlagsCalculator, OrganisationPeriodFlagsCalculator>();
        services.AddTransient<IProducerErrorDetector, ProducerErrorDetector>();
        services.AddTransient<ProducerDataPipelineServices>();
        services.AddTransient<IProducerDataService, ProducerDataService>();

        services
            .AddOptions<DataApiLoadOptions>()
            .Configure<IServiceProvider>((options, provider) => options.Enabled = loadTablesEnabled(provider));

        services.AddDbContextFactory<DataApiLoadContext>((provider, builder) =>
            builder.UseSqlServer(loadTableConnectionString(provider)));

        services.AddTransient<ILoadTableRefresher, LoadTableRefresher>();
        services.AddTransient<SynapseDataSource>();
        services.AddTransient<LoadTableDataSource>();
        services.AddTransient<IPayCalDataSource>(provider =>
            provider.GetRequiredService<IOptions<DataApiLoadOptions>>().Value.Enabled
                ? provider.GetRequiredService<LoadTableDataSource>()
                : provider.GetRequiredService<SynapseDataSource>());

        return services;
    }
}
