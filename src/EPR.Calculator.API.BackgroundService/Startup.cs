using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.BackgroundService;
using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Builder.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Builder.CommsCost;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Builder.Lapcap;
using EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages;
using EPR.Calculator.API.BackgroundService.Builder.Modulation;
using EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.BackgroundService.Builder.ParametersOther;
using EPR.Calculator.API.BackgroundService.Builder.PartialObligations;
using EPR.Calculator.API.BackgroundService.Builder.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Detail;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Filters;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.API.BackgroundService;

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
        RegisterBillingRunDependencies(services);
        RegisterValidators(services);
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
        services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
        services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
        services.AddTransient<ICalculatorRunDataInitializer, CalculatorRunDataInitializer>();
        services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
        services.AddTransient<ICalculatorFileGenerator, CalculatorFileGenerator>();
        services.AddTransient<ICalcResultsExporter, CalcResultsExporter>();

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

    private static void RegisterBillingRunDependencies(IServiceCollection services)
    {
        services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
        services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
        services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
        services.AddTransient<IBillingBuilder, BillingBuilder>();
        services.AddTransient<IBillingFileGenerator, BillingFileGenerator>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();
    }

    private static void RegisterValidators(IServiceCollection services)
    {
        services.AddSingleton<IValidator<CalcResult>, CalcResultValidator>();
    }

    private static void RegisterCommonDependencies(IServiceCollection services)
    {
        services.AddTransient<IResultBuilder, ResultBuilder>();
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
        services.AddTransient<IProducerFeesBuilder, ProducerFeesBuilder>();
        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
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
        services.AddTransient<IProducerFeesExporter, ProducerFeesExporter>();
        services.AddTransient<ICalcResultValidationExporter, CalcResultValidationExporter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();
        services.AddTransient<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
        services.AddTransient<IErrorReportService, ErrorReportService>();
        services.AddTransient<ICalcResultReader, CalcResultReader>();
        services.AddTransient<ICalcResultWriter, CalcResultWriter>();
        services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddTransient<IReportedProducerService, ReportedProducerService>();
    }
}
