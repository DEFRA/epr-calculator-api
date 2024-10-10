using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Common.ServiceBus;
using Azure.Messaging.ServiceBus;
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
builder.Services.AddScoped<IServiceBusClientFactory, ServiceBusClientFactory>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>();
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAzureClients(builder =>
{
    builder
        .AddServiceBusClient("<< CONNECTION STRING >>")
        .WithName("primary");

    // Register a sender for the "primary" client.
    builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
        provider
            .GetService<IAzureClientFactory<ServiceBusClient>>()
            .CreateClient("primary")
            .CreateSender("someQueue")
    )
    .WithName("primary-someQueue");

    // Register a sender for the "secondary" client.
    builder.AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) =>
        provider
            .GetService<IAzureClientFactory<ServiceBusClient>>()
            .CreateClient("secondary")
            .CreateSender("someTopic")
    )
    .WithName("secondary-someTopic");
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
