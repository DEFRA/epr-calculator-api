using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

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

builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var serviceBusConnectionString = builder.Configuration.GetSection("ServiceBus").GetSection("ConnectionString");
var serviceBusQueueName = builder.Configuration.GetSection("ServiceBus").GetSection("QueueName").Value;

builder.Services.AddAzureClients(builder =>
{
    builder
        .AddServiceBusClient(serviceBusConnectionString)
        .WithName("calculator");

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
