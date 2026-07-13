using System.Reflection;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Filters;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace EPR.Calculator.API;

public static class ServiceRegistration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
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

        return services;
    }

    public static IServiceCollection AddBlobStorage(this IServiceCollection services)
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

    public static IServiceCollection AddServiceBus(this IServiceCollection services)
    {
        services
            .AddOptions<ServiceBusOptions>()
            .BindConfiguration(ServiceBusOptions.SectionKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAzureClients(builder =>
        {
            builder.AddClient<ServiceBusClient, ServiceBusClientOptions>((clientOptions, _, provider) =>
            {
                var appOptions = provider
                    .GetRequiredService<IOptions<ServiceBusOptions>>()
                    .Value;

                clientOptions.RetryOptions.MaxRetries = appOptions.MaxRetries;
                clientOptions.RetryOptions.Delay = appOptions.RetryDelay;

                return new ServiceBusClient(appOptions.ConnectionString, clientOptions);
            });
        });

        services.AddSingleton<IServiceBusService, ServiceBusService>();

        return services;
    }

    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
        services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
        services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
        services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

        return services;
    }

    public static IServiceCollection AddPayCalServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
        services.AddScoped<IBillingFileService, BillingFileService>();
        services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
        services.AddScoped<ICalculationRunService, CalculationRunService>();

        return services;
    }

    public static IServiceCollection AddPayCalAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationBuilder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
        var entraIdConfig = configuration.GetSection("AzureAd");
        var oidcConfig = configuration.GetSection("Oidc");

        if (oidcConfig.GetValue<bool>("Enabled"))
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

    public static IServiceCollection AddPayCalAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("SASuperUser")
                .Build());

        return services;
    }
}
