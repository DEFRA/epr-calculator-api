using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.Data;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface ICalculatorRunDataInitializer
{
    Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunDataInitializer(
    ApplicationDBContext dbContext,
    IDataLoader dataLoader,
    ICalculatorRunOrgData calculatorRunOrgData,
    ICalculatorRunPomData calculatorRunPomData,
    IProducerDataTransposer transposer,
    ILogger<CalculatorRunDataInitializer> logger)
    : ICalculatorRunDataInitializer
{
    [ActivityTrace]
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        // DataLoader handles its own transactions and telemetry
        await dataLoader.LoadData(runContext, cancellationToken);
        await TransposeData(runContext, cancellationToken);
    }

    [ActivityMetric(nameof(Metrics.DataDuration), threshold: "00:00:30")]
    private async Task TransposeData(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await calculatorRunOrgData.LoadData(runContext, cancellationToken);
            await calculatorRunPomData.LoadData(runContext, cancellationToken);
            await transposer.Transpose(runContext, cancellationToken);
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
