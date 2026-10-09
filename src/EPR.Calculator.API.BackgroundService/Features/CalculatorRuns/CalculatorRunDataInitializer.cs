using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.Api.DataApi.Models;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface ICalculatorRunDataInitializer
{
    Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunDataInitializer(
    ApplicationDBContext dbContext,
    ICalculatorDataApiService calculatorDataApiService,
    IProducerDataTransposer transposer,
    ILogger<CalculatorRunDataInitializer> logger)
    : ICalculatorRunDataInitializer
{
    [ActivityTrace]
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        // CalculatorDataApiService performs no persistence - it's a single request into DataApi
        // returning data in memory.
        var producerRecords = await calculatorDataApiService.GetProducerRecords(runContext, cancellationToken);
        await TransposeData(runContext, producerRecords, cancellationToken);
    }

    [ActivityMetric(nameof(Metrics.DataDuration), threshold: "00:00:30")]
    private async Task TransposeData(
        CalculatorRunContext runContext,
        IReadOnlyList<ProducerRecord> producerRecords,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await transposer.Transpose(runContext, producerRecords, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Rolling back transaction");
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RunDataInitializeException(runContext.RunType, runContext.RunId, ex);
        }
    }
}
