using System.Net.Http.Headers;
using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.CommonDataApi;
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
    /// <param name="loadTableConnectionString">Resolves the connection string of the database that hosts the data_api_load_* tables.</param>
    /// <param name="loadTablesEnabled">
    ///     Whether a run refreshes the load tables from the RPD source before reading them (true), or
    ///     reads them as they already stand, without refreshing first (false) - e.g. to run several
    ///     calculations against the same already-staged snapshot without re-querying RPD each time. A
    ///     run always reads from the load tables; this only controls whether they're refreshed first.
    ///     Resolved when the options are first read, so the choice is made at run time.
    /// </param>
    /// <param name="captureMemoryMetrics">
    ///     Whether DataApi's activity traces carry allocated/heap-bytes tags - off by default, since
    ///     that was measured expensive under Azure Monitor at production activity volumes; the
    ///     performance test turns it on. Known at startup, so this is a plain value, not a factory.
    /// </param>
    public static IServiceCollection AddDataApi(
        this IServiceCollection services,
        Func<IServiceProvider, string> loadTableConnectionString,
        Func<IServiceProvider, bool> loadTablesEnabled,
        bool captureMemoryMetrics = false)
    {
        DataApiTelemetry.CaptureMemoryMetrics = captureMemoryMetrics;

        services
            .AddOptions<CommonDataApiHttpClientOptions>()
            .BindConfiguration(CommonDataApiHttpClientOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IStreamOrganisationsRequestHandler, StreamOrganisationsRequestHandler>(ConfigureHttpClient);
        services.AddHttpClient<IStreamPomsRequestHandler, StreamPomsRequestHandler>(ConfigureHttpClient);

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
        services.AddTransient<CommonDataApiSource>();

        // A run always reads from the load tables - DataApiLoadOptions.Enabled only controls whether
        // ProducerDataService refreshes them first, not which source gets read from.
        services.AddTransient<IPayCalDataSource, LoadTableDataSource>();

        return services;
    }

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<IOptions<CommonDataApiHttpClientOptions>>().Value;

        client.BaseAddress = new Uri(options.BaseUrl);

        // Disable the built-in HttpClient timeout so that the per-request StreamStartTimeout
        // (via CancellationTokenSource, see NdJsonHttpReader) is the sole timeout.
        client.Timeout = Timeout.InfiniteTimeSpan;

        if (options.CompressionEnabled)
        {
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        }
    }
}
