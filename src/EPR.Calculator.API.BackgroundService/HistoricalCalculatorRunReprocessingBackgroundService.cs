using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using Microsoft.Extensions.DependencyInjection;
using BackgroundServiceBase = Microsoft.Extensions.Hosting.BackgroundService;

namespace EPR.Calculator.API.BackgroundService;

[ExcludeFromCodeCoverage]
public class HistoricalCalculatorRunReprocessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<HistoricalCalculatorRunReprocessingBackgroundService> logger)
    : BackgroundServiceBase
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            using var scope = scopeFactory.CreateScope();

            await scope.ServiceProvider
                .GetRequiredService<IHistoricalCalculatorRunReprocessor>()
                .ReprocessAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("Historical calculator run reprocessing cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Historical calculator run reprocessing failed");
        }
    }
}
