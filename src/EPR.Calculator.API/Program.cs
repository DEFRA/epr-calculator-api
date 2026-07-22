using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EPR.Calculator.API;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.HealthCheck;

[assembly: SuppressMessage(
    "SonarAnalyzer.CSharp",
    "S5122:Make sure this permissive CORS policy is safe here",
    Justification = "Pre-existing behaviour.")]

const string corsPolicy = "AllowAllOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

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
    .AddPayCalAuthentication(builder.Configuration)
    .AddPayCalAuthorization()
    .AddDatabase()
    .AddBlobStorage()
    .AddServiceBus()
    .AddRequestValidation()
    .AddPayCalServices();

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
