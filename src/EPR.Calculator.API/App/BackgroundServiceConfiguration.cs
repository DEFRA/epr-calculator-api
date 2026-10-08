using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService;
using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Builder.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Builder.CommsCost;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Builder.Lapcap;
using EPR.Calculator.API.BackgroundService.Builder.LateReportingTonnages;
using EPR.Calculator.API.BackgroundService.Builder.Modulation;
using EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.BackgroundService.Builder.ParametersOther;
using EPR.Calculator.API.BackgroundService.Builder.PartialObligations;
using EPR.Calculator.API.BackgroundService.Builder.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Builder.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Builder.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Detail;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Modulation;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Summary;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.Validation;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.CommonDataApi;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using FluentValidation;

namespace EPR.Calculator.API.App;

[ExcludeFromCodeCoverage]
public static class BackgroundServiceConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPayCalBackgroundServices()
        {
            // Register Telemetry - ITelemetry<T> resolves for any T without per-type registration.
            services.AddSingleton(typeof(ITelemetry<>), typeof(Telemetry<>));

            // Register BlobStorageUpload
            services
                .AddOptions<BlobStorageUploadOptions>()
                .BindConfiguration(BlobStorageUploadOptions.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IStorageUploadService, BlobStorageUploadService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<MessageProcessingBackgroundService>();

            // Register CalculatorRunDependencies
            services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
            services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
            services.AddTransient<ICalculatorRunDataInitializer, CalculatorRunDataInitializer>();
            services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
            services.AddTransient<ICalcResultsExporter, CalcResultsExporter>();

            // Register CommonDataApi
            services
                .AddOptions<CommonDataApiHttpClientOptions>()
                .BindConfiguration(CommonDataApiHttpClientOptions.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHttpClient<ICommonDataApiClient, CommonDataApiHttpClient>();

            services
                .AddOptions<CommonDataApiLoaderOptions>()
                .BindConfiguration(CommonDataApiLoaderOptions.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddTransient<IDataLoader, CommonDataApiLoader>();
            services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
            services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
            services.AddTransient<IProducerDataTransposer, ProducerDataTransposer>();

            // Register BillingRunDependencies
            services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
            services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
            services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
            services.AddTransient<IBillingFileExporter, BillingFileExporter>();
            services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();

            // Register Validators
            services.AddSingleton<IValidator<CalcResult>, CalcResultValidator>();

            // Register CommonDependencies
            services.AddTransient<IResultBuilder, ResultBuilder>();
            services.AddTransient<IParameterService, ParameterService>();
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
            services.AddTransient<IProducerFeesBuilder, ProducerFeesBuilder>();
            services.AddTransient<IBillingInstructionService, BillingInstructionService>();
            services.AddTransient<ICalcResultOnePlusFourApportionmentExporter, CalcResultOnePlusFourApportionmentExporter>();
            services.AddTransient<ICalcResultDetailExporter, CalcResultDetailExporter>();
            services.AddTransient<ICalcResultLapcapDataExporter, CalcResultLapcapDataExporter>();
            services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
            services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
            services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
            services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
            services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
            services.AddTransient<CalcResultLateReportingExporter, CalcResultLateReportingExporter>();
            services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
            services.AddTransient<ICalcResultModulationExporter, CalcResultModulationExporter>();
            services.AddTransient<ICalcResultCommsCostExporter, CalcResultCommsCostExporter>();
            services.AddTransient<IProducerFeesExporter, ProducerFeesExporter>();
            services.AddTransient<ICalcResultValidationExporter, CalcResultValidationExporter>();
            services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();
            services.AddTransient<ICalcResultLateReportingExporter, CalcResultLateReportingExporter>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
            services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
            services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
            services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
            services.AddTransient<IBillingFileExporter, BillingFileExporter>();
            services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
            services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
            services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
            services.AddTransient<IErrorReportService, ErrorReportService>();
            services.AddTransient<ICalcResultReader, CalcResultReader>();
            services.AddTransient<ICalcResultWriter, CalcResultWriter>();
            services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
            services.AddTransient<IReportedProducerService, ReportedProducerService>();

            return services;
        }
    }
}
