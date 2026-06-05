using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.DataLoading;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun;

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
    ITelemetryClient telemetry,
    ILogger<CalculatorRunDataInitializer> logger)
    : ICalculatorRunDataInitializer
{
    public async Task Initialize(CalculatorRunContext runContext, CancellationToken cancellationToken) =>
        await telemetry.TrackDuration("CalculatorRunDataInitialize", async () =>
        {
            // DataLoader handles its own dbContexts and transactions
            await dataLoader.LoadData(runContext, cancellationToken);

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await calculatorRunOrgData.LoadOrgDataForCalcRun(runContext, cancellationToken);
                await calculatorRunPomData.LoadPomDataForCalcRun(runContext, cancellationToken);
                await transposer.Transpose(runContext, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Rolling back transaction");
                await transaction.RollbackAsync(CancellationToken.None);
                throw new RunDataInitializeException(runContext.RunType, runContext.RunId, ex);
            }
        });
}
