using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.HealthCheck;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
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
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;
var IsRunningLocally = environmentName.Equals(CommonResources.Local, StringComparison.OrdinalIgnoreCase);

//
// Configuration
//
builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

//
// Logging
//
builder.Host.UseSerilog((ctx, services, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .ReadFrom.Services(services)
      .Enrich.FromLogContext();

    if (IsRunningLocally) {
      lc.WriteTo.Console();
    } else {
      lc.WriteTo.ApplicationInsights(services.GetRequiredService<Microsoft.ApplicationInsights.TelemetryClient>(), TelemetryConverter.Traces);
    }
});

//
// Core ASP.NET
//
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//
// Authentication / Authorization
//
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(CommonResources.SASuperUserRole)
        .Build());

//
// FluentValidation
//
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();

//
// Database
//
builder.Services.AddDbContext<ApplicationDBContext>((provider, options) =>
{
    var dbOptions = provider
        .GetRequiredService<IOptions<DatabaseOptions>>()
        .Value;

    options.UseSqlServer(
        dbOptions.ConnectionString,
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(
                (int)dbOptions.CommandTimeout.TotalSeconds);
        });
});

//
// Blob Storage
//
builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var options = provider
        .GetRequiredService<IOptions<BlobStorageOptions>>()
        .Value;

    return new BlobServiceClient(options.ConnectionString);
});


builder.Services.AddAppDependencies(builder.Configuration);

//
// Options
//
builder.Services
    .AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionKey)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<BlobStorageOptions>()
    .BindConfiguration(BlobStorageOptions.SectionKey)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CommonDataApiHttpClientOptions>()
    .BindConfiguration(CommonDataApiHttpClientOptions.SectionKey)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CommonDataApiLoaderOptions>()
    .BindConfiguration(CommonDataApiLoaderOptions.SectionKey)
    .ValidateDataAnnotations()
    .ValidateOnStart();

//
// Timeout Policies
//
foreach (var policy in CommonResources.TimeoutPolicies.Split(','))
{
    var timeout = builder.Configuration
        .GetSection("Timeouts")
        .GetValue<double>(policy);

    builder.Services.AddRequestTimeouts(options =>
    {
        options.AddPolicy(policy, TimeSpan.FromMinutes(timeout));
    });
}

var app = builder.Build();

//
// Middleware
//
if (IsRunningLocally)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestTimeouts();

app.MapControllers();

app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build())
    .AllowAnonymous();

await app.RunAsync();



public static class DependencyInjection
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services, IConfiguration config)
    {
        //
        // Http Clients
        //
        services.AddHttpClient<ICommonDataApiClient, CommonDataApiHttpClient>();

        //
        // Core Infrastructure
        //
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();

        //
        // API Services
        //
        services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
        services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
        services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
        services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

        services.AddScoped<IOrgAndPomWrapper, OrgAndPomWrapper>();

        services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
        services.AddScoped<IBillingFileService, BillingFileService>();
        services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
        services.AddScoped<ICalculationRunService, CalculationRunService>();

        services.AddScoped<IStorageService, BlobStorageService>();
        services.AddScoped<IBlobStorageService2, BlobStorageService2>();

        services.AddScoped<ICommandTimeoutService, CommandTimeoutService>();

        //
        // Function Services
        //
        var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        if (instrumentationKey is null or "" or "00000000-0000-0000-0000-000000000000")
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();

        services.AddScoped<IDataLoader, CommonDataApiLoader>();

        services.AddScoped<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddScoped<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddScoped<IProducerDataTransposer, ProducerDataTransposer>();

        services.AddScoped<ICalculatorRunService, CalculatorRunService>();
        services.AddScoped<IRpdStatusDataValidator, RpdStatusDataValidator>();
        services.AddScoped<ICalcResultBuilder, CalcResultBuilder>();
        services.AddScoped<ICalcResultsExporter, CalcResultsExporter>();
        services.AddScoped<CalculatorRunValidator>();
        services.AddScoped<IPrepareCalcService, PrepareCalcService>();
        services.AddScoped<IParameterService, ParameterService>();
        services.AddScoped<IStorageUploadService, BlobStorageUploadService>();

        //
        // Builders
        //
        services.AddScoped<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddScoped<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddScoped<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddScoped<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddScoped<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddScoped<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddScoped<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddScoped<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddScoped<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddScoped<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddScoped<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddScoped<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddScoped<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddScoped<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddScoped<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();

        //
        // Exporters
        //
        services.AddScoped<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
        services.AddScoped<ICalcResultDetailExporter, CalcResultDetailExporter>();
        services.AddScoped<ICalcResultLapcapDataExporter, CalcResultLapcapDataExporter>();
        services.AddScoped<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddScoped<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddScoped<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddScoped<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddScoped<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddScoped<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddScoped<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddScoped<ICalcResultCommsCostExporter, CalcResultCommsCostExporter>();
        services.AddScoped<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddScoped<ICalcBillingJsonExporter, CalcResultsJsonExporter>();
        services.AddScoped<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddScoped<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddScoped<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();

        //
        // Supporting Services
        //
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<MessageProcessingBackgroundService>();

        services.AddScoped<IBillingInstructionService, BillingInstructionService>();
        services.AddScoped<IRpdStatusService, RpdStatusService>();
        services.AddScoped<IRunNameService, RunNameService>();
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IPrepareBillingFileService, PrepareBillingFileService>();
        services.AddScoped<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddScoped<IInvoicedProducerService, InvoicedProducerService>();
        services.AddScoped<IBillingFileExporter, BillingFileExporter>();
        services.AddScoped<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddScoped<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
        services.AddScoped<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();
        services.AddScoped<IErrorReportService, ErrorReportService>();
        services.AddScoped<IProjectedProducersService, ProjectedProducersService>();
        services.AddScoped<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddScoped<IReportedProducerService, ReportedProducerService>();

        return services;
    }
}