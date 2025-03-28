using System.Configuration;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
builder.Services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
builder.Services.AddScoped<IOrgAndPomWrapper, OrgAndPomWrapper>();
builder.Services.AddScoped<IStorageService, BlobStorageService>();
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddScoped<ICommandTimeoutService, CommandTimeoutService>();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();

// Configure the database context.
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("BlobStorage:ConnectionString").Value;
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");
    }

    return new BlobServiceClient(connectionString);
});

var serviceBusConnectionString = builder.Configuration.GetSection("ServiceBus").GetSection("ConnectionString");
var serviceBusQueueName = builder.Configuration.GetSection("ServiceBus").GetSection("QueueName").Value;
#pragma warning disable CS8604 // Possible null reference argument.
var retryPeriod = double.Parse(builder.Configuration.GetSection("ServiceBus").GetSection("PostMessageRetryPeriod").Value);
var retryCount = int.Parse(builder.Configuration.GetSection("ServiceBus").GetSection("PostMessageRetryCount").Value);
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddAzureClients(builder =>
{
    builder
        .AddServiceBusClient(serviceBusConnectionString)
        .WithName("calculator")
        .ConfigureOptions(options =>
        {
            options.RetryOptions.Delay = TimeSpan.FromSeconds(retryPeriod);
            options.RetryOptions.MaxDelay = TimeSpan.FromSeconds(retryPeriod);
            options.RetryOptions.MaxRetries = retryCount;
        });

    // Register a sender for the "calculator" client.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
        provider
            .GetService<IAzureClientFactory<ServiceBusClient>>()
            .CreateClient("calculator")
            .CreateSender(serviceBusQueueName))
    .WithName($"calculator-{serviceBusQueueName}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CommonConstants.PolicyName,
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Content-Disposition"));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure endpoint timeout policies.
foreach (string policy in TimeoutPolicies.AllPolicies)
{
    var timeout = builder.Configuration.GetSection("Timeouts").GetValue<double>(policy);
    builder.Services.AddRequestTimeouts(options =>
    {
        options.AddPolicy(policy, TimeSpan.FromMinutes(timeout));
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.UseCors(CommonConstants.PolicyName);
app.UseRequestTimeouts();

await app.RunAsync();
