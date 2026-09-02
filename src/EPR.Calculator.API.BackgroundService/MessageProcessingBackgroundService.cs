using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using Microsoft.Extensions.DependencyInjection;
using BackgroundServiceBase = Microsoft.Extensions.Hosting.BackgroundService;

namespace EPR.Calculator.API.BackgroundService;

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
    ILogger<MessageProcessingBackgroundService> logger,
    ITelemetry<MessageProcessingBackgroundService> telemetry)
    : BackgroundServiceBase
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

            return message.MessageType switch
            {
                BackgroundServiceMessageType.Result  => await serviceProvider.GetRequiredService<ICalculatorRunContextBuilder>().Build(message.CalculatorRunId, message.Username, ct),
                BackgroundServiceMessageType.Billing => await serviceProvider.GetRequiredService<IBillingRunContextBuilder>().Build(message.CalculatorRunId, message.Username, ct),
                _ => throw new RunContextException(RunType.Unknown, message.CalculatorRunId, $"Invalid message type: {message.MessageType}")
            };
        }
        catch (RunContextException ex)
        {
            logger.LogError(ex, "Run initialization failed for: '{Message}'", message);
            return null;
        }
    }

    private async Task ProcessRun(IServiceProvider serviceProvider, RunContext runContext, long startTime, CancellationToken ct)
    {
        using (logger.BeginRunScope(runContext))
        using (telemetry.BeginRunScope(runContext))
        {
            var result = await DoProcessing();
            var duration = Stopwatch.GetElapsedTime(startTime);

            if (result.Succeeded)
                logger.LogInformation("Run succeeded. Duration: {Duration}", duration);
            else
                logger.LogError("Run FAILED. Duration: {Duration}", duration);
        }

        Task<RunResult> DoProcessing() =>
            runContext switch
            {
                CalculatorRunContext calculatorRunContext => serviceProvider.GetRequiredService<ICalculatorRunProcessor>().Process(calculatorRunContext, ct),
                BillingRunContext billingRunContext => serviceProvider.GetRequiredService<IBillingRunProcessor>().Process(billingRunContext, ct),
                _ => throw new ArgumentException("Invalid runContext type: " + runContext.GetType().Name)
            };
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record BackgroundServiceMessage
{
    public required BackgroundServiceMessageType MessageType { get; init; }
    public required int CalculatorRunId { get; init; }
    public required string? Username { get; init; }
}

public enum BackgroundServiceMessageType
{
    Result,
    Billing
}
