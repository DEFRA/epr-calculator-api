using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Detail;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.EPR.Calculator.API.Services;
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
builder.Services.AddScoped<IRpdStatusDataValidator, RpdStatusDataValidator>();
builder.Services.AddScoped<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
builder.Services.AddScoped<ICalcResultBuilder, CalcResultBuilder>();
builder.Services.AddScoped<ICalcResultsExporter<CalcResult>, CalcResultsExporter>();
builder.Services.AddScoped<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
builder.Services.AddScoped<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
builder.Services.AddScoped<IStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ITransposePomAndOrgDataService, TransposePomAndOrgDataService>();
builder.Services.AddScoped<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
builder.Services.AddScoped<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
builder.Services.AddScoped<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
builder.Services.AddScoped<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
builder.Services.AddScoped<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();


// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetSection("BlobStorage:ConnectionString").Value;
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentNullException("BlobStorage:ConnectionString", "Blob Storage connection string is not configured.");
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
            .CreateSender(serviceBusQueueName)
    )
    .WithName($"calculator-{serviceBusQueueName}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
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

app.Run();
