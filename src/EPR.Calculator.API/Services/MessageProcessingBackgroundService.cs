using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using EPR.Calculator.API.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry.Helpers;

namespace EPR.Calculator.API.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(MessageBase message, CancellationToken ct = default);
    ValueTask<MessageBase> DequeueAsync(CancellationToken ct);
}

[ExcludeFromCodeCoverage]
public class BackgroundTaskQueue: IBackgroundTaskQueue
{
    private readonly Channel<MessageBase> _channel =
        Channel.CreateBounded<MessageBase>(1);

    public ValueTask QueueAsync(MessageBase message, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(message, ct);

    public ValueTask<MessageBase> DequeueAsync(CancellationToken ct)
        => _channel.Reader.ReadAsync(ct);
}

[ExcludeFromCodeCoverage]
public class CalculatorRunState
{
    public MessageBase? Current { get; private set; }

    public void Set(MessageBase message)
        => Current = message;

    public MessageBase Take()
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
            var runContext = await backgroundTaskQueue.DequeueAsync(stoppingToken);
            try
            {
                var startTime = Stopwatch.GetTimestamp();
                logger.LogInformation("Run initializing for message Id:'{Id}' Type:'{Type}'", runContext.CalculatorRunId, runContext.MessageType);
                telemetry.TrackEvent(TelemetryEvents.RunInit());

                telemetry.TrackEvent(TelemetryEvents.RunStarted(runContext));
                await ProcessRun(runContext, startTime);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Run initialization failed for message Id:'{Id}' Type:'{Type}'", runContext.CalculatorRunId, runContext.MessageType);
                telemetry.TrackEvent(TelemetryEvents.RunInitFailed(runContext.CalculatorRunId.ToString()));
            }
        }
    }

    private async Task ProcessRun(MessageBase runContext, long startTime)
    {
        using var scope = scopeFactory.CreateScope();

        using (logger.BeginScope(runContext.Summary))
        {
            try
            {
                var success = await ProcessRun(runContext, scope.ServiceProvider);

                if (success)
                    HandleSuccess(runContext, Stopwatch.GetElapsedTime(startTime));
                else
                    await HandleFailed(runContext, Stopwatch.GetElapsedTime(startTime), scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                await HandleFailed(runContext, Stopwatch.GetElapsedTime(startTime), scope.ServiceProvider, ex);
            }
        }
    }

    private async Task<bool> ProcessRun(MessageBase runContext, IServiceProvider serviceProvider)
    {
        switch (runContext)
        {
            case CalculatorRunMessage msg:
            {
                logger.LogInformation("Processing calculator run");

                var result = await serviceProvider
                    .GetRequiredService<ICalculatorRunService>()
                    .PrepareResultsFileAsync(msg);

                return result.IsSuccess;
            }

            case BillingFileGenerationMessage msg:
            {
                logger.LogInformation("Processing billing run");

                var result = await serviceProvider
                    .GetRequiredService<IPrepareBillingFileService>()
                    .PrepareBillingFileAsync(calculatorRunId: msg.CalculatorRunId, runName: msg.RunName, approvedBy: msg.ApprovedBy);

                return result.IsSuccess;
            }

            default:
                throw new ArgumentException($"Invalid message type: {runContext.GetType().Name}", nameof(runContext));
        }
    }

    private void HandleSuccess(MessageBase runContext, TimeSpan elapsed)
    {
        logger.LogInformation("Run completed successfully. Duration: {Duration}", elapsed);
        telemetry.TrackEvent(TelemetryEvents.RunCompleted(runContext, elapsed));
        telemetry.TrackDuration($"{runContext.MessageType}Run", elapsed);
    }

    private async Task HandleFailed(MessageBase runContext, TimeSpan elapsed, IServiceProvider serviceProvider, Exception? exception = null)
    {
        try
        {
            if (exception is not null)
                logger.LogError(exception, "Run failed due to unhandled exception");
            else
                logger.LogError(exception, "Run failed due to processor returning FALSE");

            telemetry.TrackEvent(TelemetryEvents.RunFailed(runContext, elapsed, "ProcessingFailed"));
            telemetry.TrackDuration($"{runContext.MessageType}Run", elapsed);

            // Maintains current behaviour of billing run failure setting the run classification to ERROR
            await serviceProvider
                .GetRequiredService<IClassificationService>()
                .UpdateRunClassification(runContext.CalculatorRunId, RunClassification.ERROR);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update run classification to ERROR");
        }
    }

}