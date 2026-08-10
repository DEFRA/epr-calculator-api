using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Filters;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns;
using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.Lapcap;
using EPR.Calculator.API.BackgroundService.Builder.ParametersOther;
using EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.BackgroundService.Builder.CommsCost;
using EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages;
using EPR.Calculator.API.BackgroundService.Builder.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Builder.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Builder.PartialObligations;
using EPR.Calculator.API.BackgroundService.Builder.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.Modulation;
using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Detail;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Builder.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;
using EPR.Calculator.API.BackgroundService;

namespace EPR.Calculator.API;

public static class ServiceRegistration
{
    public static bool HasApplicationInsights()
    {
        // APPINSIGHTS_INSTRUMENTATIONKEY is obsolete but still in use for now
        var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
        return instrumentationKey is not null and not "" and not "00000000-0000-0000-0000-000000000000";
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        if (HasApplicationInsights())
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // When running on AspNetCore the factory defaults to creating contexts with Scoped lifetime.
        services.AddDbContextFactory<ApplicationDBContext>((provider, builder) =>
        {
            var appOptions = provider
                .GetRequiredService<IOptions<DatabaseOptions>>()
                .Value;

            builder.UseSqlServer(
                appOptions.ConnectionString,
                sqlOptions => { sqlOptions.CommandTimeout((int)appOptions.CommandTimeout.TotalSeconds); });
        });

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();

        return services;
    }

    public static IServiceCollection AddBlobStorage(this IServiceCollection services)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .BindConfiguration(BlobStorageOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAzureClients(builder =>
        {
            builder.AddClient<BlobServiceClient, BlobClientOptions>((_, _, provider) =>
            {
                var appOptions = provider
                    .GetRequiredService<IOptions<BlobStorageOptions>>()
                    .Value;

                return new BlobServiceClient(appOptions.ConnectionString);
            });
        });

        services.AddSingleton<IBlobStorageService, BlobStorageService>();

        return services;
    }

    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.Configure<MvcOptions>(options =>
            options.Filters.Add<FluentValidationActionFilter>());

        services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
        services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
        services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
        services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

        return services;
    }

    public static IServiceCollection AddPayCalServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
        services.AddScoped<IBillingFileService, BillingFileService>();
        services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
        services.AddScoped<ICalculationRunService, CalculationRunService>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Register CoreDependencies
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        // Register BlobStorageUpload
        services
            .AddOptions<BlobStorageUploadOptions>()
            .BindConfiguration(BlobStorageUploadOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IStorageUploadService, BlobStorageUploadService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<MessageProcessingBackgroundService>();

        // Register CalculatorRunDependencies
        services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
        services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
        services.AddTransient<ICalculatorRunDataInitializer, CalculatorRunDataInitializer>();
        services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
        services.AddTransient<ICalculatorFileGenerator, CalculatorFileGenerator>();
        services.AddTransient<ICalcResultsExporter, CalcResultsExporter>();

        // Register CommonDataApi
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

        // Register BillingRunDependencies
        services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
        services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
        services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
        services.AddTransient<IBillingBuilder, BillingBuilder>();
        services.AddTransient<IBillingFileGenerator, BillingFileGenerator>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();

        // Register Validators
        services.AddSingleton<IValidator<CalcResult>, CalcResultValidator>();

        // Register CommonDependencies
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

        return services;
    }

    public static IServiceCollection AddPayCalAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        var entraIdConfig = configuration.GetSection("AzureAd");
        var oidcConfig = configuration.GetSection("Oidc");

        if (oidcConfig.GetValue<bool>("Enabled"))
        {
            authenticationBuilder.AddJwtBearer(options =>
            {
                options.Authority = oidcConfig["Authority"];
                options.TokenValidationParameters.ValidateAudience = false;
            });
        }
        else
            authenticationBuilder.AddMicrosoftIdentityWebApi(entraIdConfig);

        return services;
    }

    public static IServiceCollection AddPayCalAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("SASuperUser")
                .Build());

        return services;
    }
}
