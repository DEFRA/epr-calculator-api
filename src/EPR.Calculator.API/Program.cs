using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.HealthCheck;
using EPR.Calculator.API.Validators;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

var environmentName = builder.Environment.EnvironmentName?.ToLower() ?? string.Empty;
var IsRunningLocally = environmentName.Equals(CommonResources.Local, StringComparison.OrdinalIgnoreCase);

//
// Configuration
//
builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((ctx, services, lc) =>
{
    var telemetrySource = Matching.FromSource(typeof(LoggerTelemetryClient).FullName!);

    lc.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (IsRunningLocally) {
        lc
        .WriteTo.Logger(lc => lc.Filter.ByExcluding(telemetrySource).WriteTo.Console(DevConsole.Logger()))
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(telemetrySource).MinimumLevel.Verbose().WriteTo.Console(DevConsole.Telemetry()));
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


builder.Services.AddAppDependencies();

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