using System.Globalization;
using System.Reflection;
using EPR.Calculator.API.App;

var enGb = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = enGb;
CultureInfo.DefaultThreadCurrentUICulture = enGb;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

builder.ConfigurePayCalLogging();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services
    .AddPayCalTelemetry(builder.Environment)
    .AddPayCalProblemDetails(builder.Environment)
    .AddPayCalAuthentication(builder.Configuration, builder.Environment)
    .AddPayCalAuthorization()
    .AddPayCalRequestValidation()
    .AddPayCalDatabase()
    .AddPayCalBlobStorage()
    .AddPayCalServices()
    .AddPayCalBackgroundServices()
    .AddPayCalFeatureFlags();

var corsPolicyName = builder.Services.AddPayCalCorsPolicy();

var app = builder.Build();
app.UsePayCalRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UsePayCalHealthChecks();
app.UsePayCalApiExplorer();

await app.RunAsync();
