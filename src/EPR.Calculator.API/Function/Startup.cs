using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Filters;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

/// <summary>
///     Configures the startup for the Azure Functions.
/// </summary>
[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    private static readonly bool IsRunningLocally = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
            .SetBasePath(Environment.CurrentDirectory)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables()
            .Build();
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        if (IsRunningLocally)
            ConfigureLocalLogging(builder);

        builder.Services.AddAppDependencies();
    }

    private static void ConfigureLocalLogging(IFunctionsHostBuilder builder)
    {
        var cfg = builder.GetContext().Configuration;

        if (!cfg.GetSection("Serilog").Exists())
            return;

        var telemetrySource = Matching.FromSource(typeof(LoggerTelemetryClient).FullName!);

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(cfg)
            .Enrich.FromLogContext()
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(telemetrySource)
                .WriteTo.Console(DevConsole.Logger()))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(telemetrySource)
                .MinimumLevel.Verbose()
                .WriteTo.Console(DevConsole.Telemetry()));

        Log.Logger = loggerConfig.CreateLogger();
        builder.Services.AddLogging(logging => logging.AddSerilog(Log.Logger, true));
    }
}

[ExcludeFromCodeCoverage]
internal static class ServiceRegistration
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        RegisterCoreDependencies(services);
        RegisterTelemetry(services);
        RegisterDatabase(services);
        RegisterBlobStorage(services);
        RegisterCalculatorRunDependencies(services);
        RegisterCommonDependencies(services);

        return services;
    }

    private static void RegisterCoreDependencies(IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
    }

    private static void RegisterTelemetry(IServiceCollection services)
    {
        var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        if (instrumentationKey is null or "" or "00000000-0000-0000-0000-000000000000")
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContextFactory<ApplicationDBContext>((provider, builder) =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            builder.UseSqlServer(
                options.ConnectionString,
                sqlOptions => { sqlOptions.CommandTimeout((int)options.CommandTimeout.TotalSeconds); });
        });

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();
    }

    private static void RegisterBlobStorage(IServiceCollection services)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .BindConfiguration(BlobStorageOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddSingleton<IStorageService, BlobStorageService>();
    }

    private static void RegisterCalculatorRunDependencies(IServiceCollection services)
    {
        services
            .AddOptions<CommonDataApiHttpClientOptions>()
            .BindConfiguration(CommonDataApiHttpClientOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ICommonDataApiClient, CommonDataApiHttpClient>();

        services
            .AddOptions<CommonDataApiLoaderOptions>()
            .BindConfiguration(CommonDataApiLoaderOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IDataLoader, CommonDataApiLoader>();

        services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddTransient<IProducerDataTransposer, ProducerDataTransposer>();
    }

    private static void RegisterCommonDependencies(IServiceCollection services)
    {
        services.AddTransient<ICalculatorRunService, CalculatorRunService>();
        services.AddTransient<IRpdStatusDataValidator, RpdStatusDataValidator>();
        services.AddTransient<IOrgAndPomWrapper, OrgAndPomWrapper>();
        services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
        services.AddTransient<ICalcResultsExporter, CalcResultsExporter>();
        services.AddTransient<CalculatorRunValidator, CalculatorRunValidator>();
        services.AddTransient<IPrepareCalcService, PrepareCalcService>();
        services.AddTransient<IParameterService, ParameterService>();
        services.AddTransient<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddTransient<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddTransient<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddTransient<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddTransient<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddTransient<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddTransient<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddTransient<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddTransient<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddTransient<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
        services.AddTransient<IRpdStatusService, RpdStatusService>();
        services.AddTransient<ICalcResultDetailExporter, CalcResultDetailExporter>();
        services.AddTransient<ICalcResultLapcapDataExporter, CalcResultLapcapDataExporter>();
        services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddTransient<CalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddTransient<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddTransient<ICalcResultCommsCostExporter, CalcResultCommsCostExporter>();
        services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddTransient<ICalcBillingJsonExporter, CalcResultsJsonExporter>();
        services.AddTransient<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddTransient<IRunNameService, RunNameService>();
        services.AddTransient<IClassificationService, ClassificationService>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<IMessageTypeService, MessageTypeService>();
        services.AddTransient<IPrepareBillingFileService, PrepareBillingFileService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
        services.AddTransient<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
        services.AddTransient<IErrorReportService, ErrorReportService>();
        services.AddTransient<IProjectedProducersService, ProjectedProducersService>();
        services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddTransient<IReportedProducerService, ReportedProducerService>();
    }
}
