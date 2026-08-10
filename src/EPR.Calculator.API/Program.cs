using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EPR.Calculator.API;
using EPR.Calculator.API.BackgroundService.Logging;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.HealthCheck;
using Microsoft.ApplicationInsights;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

[assembly: SuppressMessage(
    "SonarAnalyzer.CSharp",
    "S5122:Make sure this permissive CORS policy is safe here",
    Justification = "Pre-existing behaviour.")]

const string corsPolicy = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((ctx, services, logger) =>
{
    var telemetrySource = Matching.FromSource(typeof(LoggerTelemetryClient).FullName!);

    logger.ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (ctx.HostingEnvironment.IsLocal())
    {
        // Use human readable DevConsole for local envs
        logger
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(telemetrySource)
                .WriteTo.Console(DevConsole.Logger()))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(telemetrySource)
                .MinimumLevel.Verbose()
                .WriteTo.Console(DevConsole.Telemetry()));
    }
    else
    {
        // Other envs should still log to console (for live log sessions, etc)
        logger.WriteTo.Console(new RenderedCompactJsonFormatter());
    }

    // Forward logs to app insights if available
    if (ServiceRegistration.HasApplicationInsights())
        logger.WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryClient>(), TelemetryConverter.Traces);
});

// Framework services.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options => options.AddPolicy(
    corsPolicy,
    policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

// EPR services.
builder.Services
    .AddTelemetry()
    .AddPayCalAuthentication(builder.Configuration)
    .AddPayCalAuthorization()
    .AddDatabase()
    .AddBlobStorage()
    .AddRequestValidation()
    .AddPayCalServices()
    .AddBackgroundServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
    app.UseSwagger().UseSwaggerUI();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

await app.RunAsync();
