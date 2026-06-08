using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Features.BillingRun;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.BillingRun.Outputs;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
using EPR.Calculator.Service.Function.Telemetry;

namespace EPR.Calculator.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        //
        // Http Clients
        //
        services.AddHttpClient<ICommonDataApiClient, CommonDataApiHttpClient>();

        //
        // Core Infrastructure
        //
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();

        //
        // API Services
        //
        services.AddScoped<ICreateDefaultParameterDataValidator, CreateDefaultParameterDataValidator>();
        services.AddScoped<ILapcapDataValidator, LapcapDataValidator>();
        services.AddScoped<ICalcRelativeYearRequestDtoDataValidator, CalcRelativeYearRequestDtoDataValidator>();
        services.AddScoped<ICalculatorRunStatusDataValidator, CalculatorRunStatusDataValidator>();

        services.AddScoped<IOrgAndPomWrapper, OrgAndPomWrapper>();
        services.AddScoped<IBillingFileService, BillingFileService>();
        services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
        services.AddScoped<ICalculationRunService, CalculationRunService>();
        services.AddScoped<IStorageService, BlobStorageService>();
        services.AddScoped<IBlobStorageService2, BlobStorageService2>();
        services.AddScoped<ICommandTimeoutService, CommandTimeoutService>();

        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<MessageProcessingBackgroundService>();

        //
        // Function Services
        //
        services.AddTransient<IStorageUploadService, BlobStorageUploadService>();

        var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        if (instrumentationKey is null or "" or "00000000-0000-0000-0000-000000000000")
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();

        services.AddTransient<IDataLoader, CommonDataApiLoader>();
        services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddTransient<IProducerDataTransposer, ProducerDataTransposer>();

        //
        // Calculator Run Context
        //
        services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
        services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
        services.AddTransient<ICalculatorRunDataInitializer, CalculatorRunDataInitializer>();
        services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
        services.AddTransient<ICalculatorFileGenerator, CalculatorFileGenerator>();

        //
        // Billing Run Context
        //
        services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
        services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
        services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
        services.AddTransient<IBillingFileGenerator, BillingFileGenerator>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();

        //
        // Builders
        //
        services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
        services.AddTransient<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddTransient<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddTransient<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddTransient<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddTransient<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddTransient<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddTransient<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddTransient<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddTransient<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddTransient<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();

        //
        // Exporters
        //
        services.AddTransient<ICalcResultsExporter, CalcResultsExporter>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
        services.AddTransient<ICalcResultDetailExporter, CalcResultDetailExporter>();
        services.AddTransient<ICalcResultLapcapDataExporter, CalcResultLapcapDataExporter>();
        services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddTransient<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddTransient<ICalcResultCommsCostExporter, CalcResultCommsCostExporter>();
        services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddTransient<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();

        //
        // Supporting Services
        //
        services.AddTransient<IParameterService, ParameterService>();
        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<IInvoiceDetailsService, InvoiceDetailsService>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<IErrorReportService, ErrorReportService>();
        services.AddTransient<IProjectedProducersService, ProjectedProducersService>();
        services.AddTransient<ICalcResultService, CalcResultService>();
        services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddTransient<IReportedProducerService, ReportedProducerService>();

        return services;
    }
}