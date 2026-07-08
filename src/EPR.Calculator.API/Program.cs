using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.HealthCheck;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Validators;
using EPR.Calculator.Service.Function.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

[assembly: SuppressMessage(
    "SonarAnalyzer.CSharp",
    "S5122:Make sure this permissive CORS policy is safe here",
    Justification = "Pre-existing behaviour.")]

const string corsPolicy = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

// Framework services.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Authentication and authorisation.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(CommonResources.SASuperUserRole)
        .Build());

builder.Services.AddCors(options => options.AddPolicy(
    corsPolicy,
    policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Validation.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();
builder.Services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
builder.Services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
builder.Services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
builder.Services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

// Application services.
builder.Services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddScoped<IBillingFileService, BillingFileService>();
builder.Services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
builder.Services.AddScoped<ICalculationRunService, CalculationRunService>();
builder.Services.AddScoped<IStorageService, BlobStorageService>();
builder.Services.AddScoped<IBlobStorageService2, BlobStorageService2>();

// Database context.
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Blob storage.
builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection(CommonResources.BlobStorageSection));

builder.Services.AddSingleton(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>()
        .GetSection(CommonResources.BlobStorageSection)
        .GetValue<string>("ConnectionString");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");
    }

    return new BlobServiceClient(connectionString);
});

// Service Bus.
var serviceBusSection = builder.Configuration.GetSection("ServiceBus");
var retryPeriod = serviceBusSection.GetValue<double?>("PostMessageRetryPeriod")
    ?? throw new ConfigurationErrorsException("Configuration item not found: ServiceBus__PostMessageRetryPeriod");
var retryCount = serviceBusSection.GetValue<int?>("PostMessageRetryCount")
    ?? throw new ConfigurationErrorsException("Configuration item not found: ServiceBus__PostMessageRetryCount");

builder.Services.AddAzureClients(factoryBuilder =>
{
    factoryBuilder
        .AddServiceBusClient(serviceBusSection.GetSection("ConnectionString"))
        .WithName(CommonResources.ServiceBusClientName)
        .ConfigureOptions(options =>
        {
            options.RetryOptions.Delay = TimeSpan.FromSeconds(retryPeriod);
            options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(retryPeriod);
            options.RetryOptions.MaxRetries = retryCount;
        });
});

// Endpoint timeout policies.
builder.Services.AddRequestTimeouts(options =>
{
    foreach (var policy in CommonResources.TimeoutPolicies.Split(','))
    {
        var timeout = builder.Configuration.GetSection("Timeouts").GetValue<double>(policy);
        options.AddPolicy(policy, TimeSpan.FromMinutes(timeout));
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestTimeouts();

app.MapControllers();
app.MapHealthChecks("/admin/health", HealthCheckOptionsBuilder.Build()).AllowAnonymous();

await app.RunAsync();
