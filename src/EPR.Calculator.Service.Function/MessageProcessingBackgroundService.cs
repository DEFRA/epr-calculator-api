using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRun;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EPR.Calculator.API.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(BackgroundServiceMessage message, CancellationToken ct = default);
    ValueTask<BackgroundServiceMessage> DequeueAsync(CancellationToken ct);
}

[ExcludeFromCodeCoverage]
public class BackgroundTaskQueue: IBackgroundTaskQueue
{
    private readonly Channel<BackgroundServiceMessage> _channel =
        Channel.CreateBounded<BackgroundServiceMessage>(1);

    public ValueTask QueueAsync(BackgroundServiceMessage message, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(message, ct);

    public ValueTask<BackgroundServiceMessage> DequeueAsync(CancellationToken ct)
        => _channel.Reader.ReadAsync(ct);
}

[ExcludeFromCodeCoverage]
public class CalculatorRunState
{
    public BackgroundServiceMessage? Current { get; private set; }

    public void Set(BackgroundServiceMessage message)
        => Current = message;

    public BackgroundServiceMessage Take()
    {
        var msg = Current ?? throw new InvalidOperationException("No job queued");
        Current = null;
        return msg;
    }
}

[ExcludeFromCodeCoverage]
public class MessageProcessingBackgroundService(
    IBackgroundTaskQueue backgroundTaskQueue,
    IServiceScopeFactory scopeFactory,
    ITelemetryClient telemetry,
    ILogger<MessageProcessingBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await backgroundTaskQueue.DequeueAsync(stoppingToken);

            try
            {
                var startTime = Stopwatch.GetTimestamp();
                using var scope = scopeFactory.CreateScope();

                var runContext = await InitializeRun(scope.ServiceProvider, message, stoppingToken);

                if (runContext != null)
                    await ProcessRun(scope.ServiceProvider, runContext, startTime, stoppingToken);
            }
            catch (Exception ex)
            {
                // Exceptions should already have been handled within the processors.
                // So if we're here, it's likely due to service misconfiguration.
                logger.LogCritical(ex, "Run failed (unhandled exception)");
            }
        }
    }

    private async Task<RunContext?> InitializeRun(IServiceProvider serviceProvider, BackgroundServiceMessage message, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Run initializing for message: '{Message}'", message);
            telemetry.TrackEvent(TelemetryEvents.RunInit());

            return message.MessageType switch
            {
                "Result" => await serviceProvider.GetRequiredService<ICalculatorRunContextBuilder>().Build(message.CalculatorRunId, message.CreatedBy, ct),
                "Billing" => await serviceProvider.GetRequiredService<IBillingRunContextBuilder>().Build(message.CalculatorRunId, message.ApprovedBy, ct),
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

    private async Task ProcessRun(IServiceProvider serviceProvider, RunContext runContext, long startTime, CancellationToken ct)
    {
        using (logger.BeginRunScope(runContext))
        {
            telemetry.TrackEvent(TelemetryEvents.RunStarted(runContext));

            var processingTask = runContext switch
            {
                CalculatorRunContext calculatorRunContext => serviceProvider.GetRequiredService<ICalculatorRunProcessor>().Process(calculatorRunContext, ct),
                BillingRunContext billingRunContext => serviceProvider.GetRequiredService<IBillingRunProcessor>().Process(billingRunContext, ct),
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
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackgroundServiceMessage
{
    public required string MessageType { get; init; }
    public required int CalculatorRunId { get; init; }
    public required string? CreatedBy { get; init; }
    public required string? ApprovedBy { get; init; }
}