using System.Threading.Channels;
using EPR.Calculator.API.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.API.Services;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(MessageBase message, CancellationToken ct = default);
    ValueTask<MessageBase> DequeueAsync(CancellationToken ct);
}

public class BackgroundTaskQueue: IBackgroundTaskQueue
{
    private readonly Channel<MessageBase> _channel =
        Channel.CreateBounded<MessageBase>(1);

    public ValueTask QueueAsync(MessageBase message, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(message, ct);

    public ValueTask<MessageBase> DequeueAsync(CancellationToken ct)
        => _channel.Reader.ReadAsync(ct);
}

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

public class MessageProcessingBackgroundService(
    IBackgroundTaskQueue backgroundTaskQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<MessageProcessingBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await backgroundTaskQueue.DequeueAsync(stoppingToken);
                using var scope = scopeFactory.CreateScope();
                switch (message)
                {
                    case CalculatorRunMessage msg:
                    {
                        await scope.ServiceProvider
                            .GetRequiredService<ICalculatorRunService>()
                            .PrepareResultsFileAsync(msg);
                        break;
                    }

                    case BillingFileGenerationMessage msg:
                    {
                        await scope.ServiceProvider
                            .GetRequiredService<IPrepareBillingFileService>()
                            .PrepareBillingFileAsync(calculatorRunId: msg.CalculatorRunId, runName: msg.RunName, approvedBy: msg.ApprovedBy);
                        break;
                    }

                    default:
                        throw new InvalidOperationException($"Unknown message type: {message.GetType().Name}");
                }
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background job failed");
            }
        }
    }
}