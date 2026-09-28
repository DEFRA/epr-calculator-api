using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

public interface ICalculatorRunFinalizer
{
    /// <summary>
    ///     Persists any required state changes to the database, then marks the calculator run as
    ///     <see cref="RunClassification.UNCLASSIFIED" />.
    /// </summary>
    Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);

    /// <summary>
    ///     Marks the calculator run as <see cref="RunClassification.ERROR" />.
    /// </summary>
    Task FinalizeAsErrored(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage(Justification = "Not unit testable with in-memory database given its transactional nature")]
public class CalculatorRunFinalizer(
    ApplicationDBContext dbContext,
    IBillingInstructionService billingInstructionService,
    IProducerInvoiceNetTonnageService producerInvoiceNetTonnageService,
    ILogger<CalculatorRunFinalizer> logger)
    : ICalculatorRunFinalizer
{
    [ActivityTrace]
    public async Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await billingInstructionService.CreateBillingInstructions(runContext, calcResult, cancellationToken);
            await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(runContext, calcResult, cancellationToken);
            await SaveCompletedRunStatus(runContext, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Rolling back transaction");
            await transaction.RollbackAsync(CancellationToken.None);
            throw new RunFinalizeException(runContext.RunType, runContext.RunId, ex);
        }
    }

    public async Task FinalizeAsErrored(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        try
        {
            var calcRun = await dbContext
                .CalculatorRuns
                .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

            calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.ERRORID;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark calculation run as failed");
        }
    }

    private async Task SaveCompletedRunStatus(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var calcRun = await dbContext
            .CalculatorRuns
            .SingleAsync(run => run.Id == runContext.RunId, cancellationToken);

        calcRun.CalculatorRunClassificationId = RunClassificationStatusIds.UNCLASSIFIEDID;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
