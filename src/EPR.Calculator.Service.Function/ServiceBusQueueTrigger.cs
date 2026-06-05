using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRun;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Telemetry.Helpers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

/// <summary>
///     This is the entry point for the PayCal service. It triggers the calculator/billing run processes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "To be re-added later.")]
public class ServiceBusQueueTrigger(
    ICalculatorRunContextBuilder calculatorRunContextBuilder,
    ICalculatorRunProcessor calculatorRunProcessor,
    IBillingRunContextBuilder billingRunContextBuilder,
    IBillingRunProcessor billingRunProcessor,
    ITelemetryClient telemetry,
    ILogger<ServiceBusQueueTrigger> logger)
{
    [FunctionName("EPRCalculatorRunServiceBusQueueTrigger")]
    public async Task Run(
        [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]
        ServiceBusMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var startTime = Stopwatch.GetTimestamp();
            var runContext = await InitializeRun(message, cancellationToken);

            if (runContext != null)
                await ProcessRun(runContext, startTime, cancellationToken);
        }
        catch (Exception ex)
        {
            // Exceptions should already have been handled within the processors.
            // So if we're here, it's likely due to service misconfiguration.
            logger.LogCritical(ex, "Run failed (unhandled exception)");
        }
    }

    private async Task<RunContext?> InitializeRun(ServiceBusMessage message, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Run initializing for message: '{Message}'", message);
            telemetry.TrackEvent(TelemetryEvents.RunInit());

            return message.MessageType switch
            {
                "Result" => await calculatorRunContextBuilder.Build(message.CalculatorRunId, message.CreatedBy, ct),
                "Billing" => await billingRunContextBuilder.Build(message.CalculatorRunId, message.ApprovedBy, ct),
                _ => throw new RunContextException(RunType.Unknown, message.CalculatorRunId, $"Invalid message type: {message.MessageType}")
            };
        }
        catch (RunContextException ex)
        {
            logger.LogError(ex, "Run initialization failed for: '{Message}'", message);
            telemetry.TrackEvent(TelemetryEvents.RunInitFailed(message.ToString()));
            return null;
        }
    }

    private async Task ProcessRun(RunContext runContext, long startTime, CancellationToken ct)
    {
        using (logger.BeginRunScope(runContext))
        {
            telemetry.TrackEvent(TelemetryEvents.RunStarted(runContext));

            var processingTask = runContext switch
            {
                BillingRunContext billingRunContext => billingRunProcessor.Process(billingRunContext, ct),
                CalculatorRunContext calculatorRunContext => calculatorRunProcessor.Process(calculatorRunContext, ct),
                _ => throw new ArgumentException("Invalid runContext type: " + runContext.GetType().Name)
            };

            var result = await processingTask;
            var duration = Stopwatch.GetElapsedTime(startTime);

            if (result.Succeeded)
                HandleSucceeded(duration);
            else
                HandleFailed(duration, (BadResult)result);
        }

        void HandleSucceeded(TimeSpan duration)
        {
            logger.LogInformation("Run succeeded. Duration: {Duration}", duration);
            telemetry.TrackEvent(TelemetryEvents.RunSucceeded(runContext, duration));
            telemetry.TrackDuration($"{runContext.RunType}Run", duration);
        }

        void HandleFailed(TimeSpan duration, BadResult result)
        {
            logger.LogError("Run FAILED. Duration: {Duration}", duration);
            telemetry.TrackEvent(TelemetryEvents.RunFailed(runContext, duration, result.Exception is OperationCanceledException ? "Cancelled" : "ProcessingFailed"));
            telemetry.TrackDuration($"{runContext.RunType}Run", duration);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record ServiceBusMessage
    {
        public required string MessageType { get; init; }
        public required int CalculatorRunId { get; init; }
        public required string? CreatedBy { get; init; }
        public required string? ApprovedBy { get; init; }
    }
}
