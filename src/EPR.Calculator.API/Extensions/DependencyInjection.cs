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
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services, IConfiguration config)
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

        services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
        services.AddScoped<IBillingFileService, BillingFileService>();
        services.AddScoped<IAvailableClassificationsService, AvailableClassificationsService>();
        services.AddScoped<ICalculationRunService, CalculationRunService>();

        services.AddScoped<IStorageService, BlobStorageService>();
        services.AddScoped<IBlobStorageService2, BlobStorageService2>();

        services.AddScoped<ICommandTimeoutService, CommandTimeoutService>();

        //
        // Function Services
        //
        var instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        if (instrumentationKey is null or "" or "00000000-0000-0000-0000-000000000000")
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();

        services.AddScoped<IDataLoader, CommonDataApiLoader>();

        services.AddScoped<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddScoped<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddScoped<IProducerDataTransposer, ProducerDataTransposer>();

        services.AddScoped<ICalculatorRunService, CalculatorRunService>();
        services.AddScoped<IRpdStatusDataValidator, RpdStatusDataValidator>();
        services.AddScoped<ICalcResultBuilder, CalcResultBuilder>();
        services.AddScoped<ICalcResultsExporter, CalcResultsExporter>();
        services.AddScoped<CalculatorRunValidator>();
        services.AddScoped<IPrepareCalcService, PrepareCalcService>();
        services.AddScoped<IParameterService, ParameterService>();
        services.AddScoped<IStorageUploadService, BlobStorageUploadService>();

        //
        // Builders
        //
        services.AddScoped<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddScoped<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddScoped<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddScoped<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddScoped<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddScoped<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddScoped<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddScoped<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddScoped<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddScoped<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddScoped<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddScoped<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddScoped<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddScoped<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddScoped<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();

        //
        // Exporters
        //
        services.AddScoped<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
        services.AddScoped<ICalcResultDetailExporter, CalcResultDetailExporter>();
        services.AddScoped<ICalcResultLapcapDataExporter, CalcResultLapcapDataExporter>();
        services.AddScoped<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddScoped<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddScoped<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddScoped<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddScoped<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddScoped<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddScoped<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddScoped<ICalcResultCommsCostExporter, CalcResultCommsCostExporter>();
        services.AddScoped<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddScoped<ICalcBillingJsonExporter, CalcResultsJsonExporter>();
        services.AddScoped<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
        services.AddScoped<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddScoped<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();

        //
        // Supporting Services
        //
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddHostedService<MessageProcessingBackgroundService>();

        services.AddScoped<IBillingInstructionService, BillingInstructionService>();
        services.AddScoped<IRpdStatusService, RpdStatusService>();
        services.AddScoped<IRunNameService, RunNameService>();
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IPrepareBillingFileService, PrepareBillingFileService>();
        services.AddScoped<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddScoped<IInvoicedProducerService, InvoicedProducerService>();
        services.AddScoped<IBillingFileExporter, BillingFileExporter>();
        services.AddScoped<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddScoped<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
        services.AddScoped<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();
        services.AddScoped<IErrorReportService, ErrorReportService>();
        services.AddScoped<IProjectedProducersService, ProjectedProducersService>();
        services.AddScoped<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddScoped<IReportedProducerService, ReportedProducerService>();

        return services;
    }
}