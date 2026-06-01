using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry.Helpers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

/// <summary>
///     This is the entry point for the PayCal service. It triggers the calculator/billing run processes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "To be re-added later.")]
[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class ServiceBusQueueTrigger(
    ICalculatorRunService calculatorRunService,
    IRunNameService runNameService,
    IMessageTypeService messageTypeService,
    IPrepareBillingFileService prepareBillingFileService,
    IClassificationService classificationService,
    ITelemetryClient telemetry,
    ILogger<ServiceBusQueueTrigger> logger)
{
    [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
        string serviceBusMessage)
    {
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            logger.LogInformation("Run initializing for message: '{Message}'", serviceBusMessage);
            telemetry.TrackEvent(TelemetryEvents.RunInit());

            var runContext = messageTypeService.DeserializeMessage(serviceBusMessage);

            telemetry.TrackEvent(TelemetryEvents.RunStarted(runContext));
            await ProcessRun(runContext, startTime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Run initialization failed for: '{Message}'", serviceBusMessage);
            telemetry.TrackEvent(TelemetryEvents.RunInitFailed(serviceBusMessage));
        }
    }

    private async Task ProcessRun(MessageBase runContext, long startTime)
    {
        using (logger.BeginScope(runContext.Summary))
        {
            try
            {
                var success = await ProcessRun(runContext);

                if (success)
                    HandleSuccess(runContext, Stopwatch.GetElapsedTime(startTime));
                else
                    await HandleFailed(runContext, Stopwatch.GetElapsedTime(startTime));
            }
            catch (Exception ex)
            {
                await HandleFailed(runContext, Stopwatch.GetElapsedTime(startTime), ex);
            }
        }
    }

    private async Task<bool> ProcessRun(MessageBase runContext)
    {
        var runName = await runNameService.GetRunNameAsync(runContext.CalculatorRunId);

        if (runContext is CreateResultFileMessage calculatorContext)
        {
            logger.LogInformation("Processing calculator run");
            return (await calculatorRunService.PrepareResultsFileAsync(calculatorContext, runName)).IsSuccess;
        }

        if (runContext is CreateBillingFileMessage billingContext)
        {
            logger.LogInformation("Processing billing run");
            return (await prepareBillingFileService.PrepareBillingFileAsync(billingContext.CalculatorRunId, runName, billingContext.ApprovedBy)).IsSuccess;
        }

        throw new ArgumentException($"Invalid message type: {runContext.GetType().Name}", nameof(runContext));
    }

    private void HandleSuccess(MessageBase runContext, TimeSpan elapsed)
    {
        logger.LogInformation("Run completed successfully. Duration: {Duration}", elapsed);
        telemetry.TrackEvent(TelemetryEvents.RunCompleted(runContext, elapsed));
        telemetry.TrackDuration($"{runContext.RunType}Run", elapsed);
    }

    private async Task HandleFailed(MessageBase runContext, TimeSpan elapsed, Exception? exception = null)
    {
        try
        {
            if (exception is not null)
                logger.LogError(exception, "Run failed due to unhandled exception");
            else
                logger.LogError(exception, "Run failed due to processor returning FALSE");

            telemetry.TrackEvent(TelemetryEvents.RunFailed(runContext, elapsed, "ProcessingFailed"));
            telemetry.TrackDuration($"{runContext.RunType}Run", elapsed);

            // Maintains current behaviour of billing run failure setting the run classification to ERROR
            await classificationService.UpdateRunClassification(runContext.CalculatorRunId, RunClassification.ERROR);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update run classification to ERROR");
        }
    }
}
