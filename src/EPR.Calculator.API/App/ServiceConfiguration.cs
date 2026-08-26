using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Storage.Blobs;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Filters;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OpenTelemetry.Trace;

namespace EPR.Calculator.API.App;

[ExcludeFromCodeCoverage]
public static class ServiceConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPayCalTelemetry(IHostEnvironment environment)
        {
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .AddProcessor<EndpointProbeActivityFilter>()
                    .AddSource(Telemetry.RootScope))
                .WithMetrics(metrics => metrics
                    .AddMeter(Telemetry.RootScope))
                .UseAzureMonitor();

            if (environment.IsLocal())
                services.AddHostedService<TelemetryLoggingHost>();

            return services;
        }

        public IServiceCollection AddPayCalProblemDetails(IHostEnvironment environment)
        {
            services.AddProblemDetails(options =>
            {
                if (environment.IsDevelopment() || environment.IsLocal())
                {
                    options.CustomizeProblemDetails = context =>
                    {
                        var exception = context.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

                        if (exception is null)
                            return;

                        context.ProblemDetails.Detail = exception.Message;
                        context.ProblemDetails.Extensions["exception"] = exception.ToString();
                    };
                }
            });

            return services;
        }

        public string AddPayCalCorsPolicy()
        {
            const string corsPolicy = "PayCalCors";

            services.AddCors(options => options.AddPolicy(
                corsPolicy,
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

            return corsPolicy;
        }

        public IServiceCollection AddPayCalRequestValidation()
        {
            services
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.Configure<MvcOptions>(options =>
                options.Filters.Add<FluentValidationActionFilter>());

            services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
            services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
            services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

            return services;
        }

        public IServiceCollection AddPayCalAuthentication(IConfiguration configuration, IHostEnvironment environment)
        {
            var authenticationBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            var entraIdConfig = configuration.GetSection("AzureAd");
            var oidcConfig = configuration.GetSection("Oidc");

            if (environment.IsLocal() && oidcConfig.GetValue<bool>("Enabled"))
            {
                authenticationBuilder.AddJwtBearer(options =>
                {
                    options.Authority = oidcConfig["Authority"];
                    options.TokenValidationParameters.ValidateAudience = false;
                });
            }
            else
                authenticationBuilder.AddMicrosoftIdentityWebApi(entraIdConfig);

            return services;
        }

        public IServiceCollection AddPayCalAuthorization()
        {
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole("SASuperUser")
                    .Build());

            return services;
        }

        public IServiceCollection AddPayCalDatabase()
        {
            services
                .AddOptions<DatabaseOptions>()
                .BindConfiguration(DatabaseOptions.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // When running on AspNetCore the factory defaults to creating contexts with Scoped lifetime.
            services.AddDbContextFactory<ApplicationDBContext>((provider, builder) =>
            {
                var appOptions = provider
                    .GetRequiredService<IOptions<DatabaseOptions>>()
                    .Value;

                builder.UseSqlServer(
                    appOptions.ConnectionString,
                    sqlOptions => { sqlOptions.CommandTimeout((int)appOptions.CommandTimeout.TotalSeconds); });
            });

            services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();

            return services;
        }

        public IServiceCollection AddPayCalBlobStorage()
        {
            services
                .AddOptions<BlobStorageOptions>()
                .BindConfiguration(BlobStorageOptions.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddAzureClients(builder =>
            {
                builder.AddClient<BlobServiceClient, BlobClientOptions>((_, _, provider) =>
                {
                    var appOptions = provider
                        .GetRequiredService<IOptions<BlobStorageOptions>>()
                        .Value;

                    return new BlobServiceClient(appOptions.ConnectionString);
                });
            });

            services.AddSingleton<IBlobStorageService, BlobStorageService>();

            return services;
        }

        public IServiceCollection AddPayCalServices()
        {
            services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

            services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
            services.AddScoped<IBillingFileService, BillingFileService>();
            services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
            services.AddScoped<ICalculationRunService, CalculationRunService>();
            services.AddScoped<IFileExportService, FileExportService>();

            return services;
        }

        public IServiceCollection AddPayCalFeatureFlags()
        {
            services
                .AddOptions<FeatureFlagOptions>()
                .BindConfiguration(FeatureFlagOptions.SectionKey);

            return services;
        }
    }
}
