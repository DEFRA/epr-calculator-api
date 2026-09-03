using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface ICalculatorRunDataInitializer
{
    Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunDataInitializer(
    ApplicationDBContext dbContext,
    IDataLoader dataLoader,
    IProducerDataTransposer transposer,
    ILogger<CalculatorRunDataInitializer> logger)
    : ICalculatorRunDataInitializer
{
    [ActivityTrace]
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        // DataLoader performs no database access - it just streams data into memory.
        var (organisations, poms) = await dataLoader.LoadData(runContext, cancellationToken);
        await TransposeData(runContext, organisations, poms, cancellationToken);
    }

    [ActivityMetric(nameof(Metrics.DataDuration), threshold: "00:00:30")]
    private async Task TransposeData(
        CalculatorRunContext runContext,
        IReadOnlyList<CalculatorRunOrganisation> organisations,
        IReadOnlyList<AlignmentPom> poms,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await transposer.Transpose(runContext, organisations, poms, cancellationToken);
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
