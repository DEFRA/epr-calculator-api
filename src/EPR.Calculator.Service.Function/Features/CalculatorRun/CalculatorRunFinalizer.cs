using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun;

public interface ICalculatorRunFinalizer
{
    /// <summary>
    ///     Persists any required state changes to the database, then marks the calculator run as
    ///     <see cref="RunClassification.UNCLASSIFIED" />.
    /// </summary>
    Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileResult exportResult, CancellationToken cancellationToken);

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
    public async Task FinalizeAsCompleted(CalculatorRunContext runContext, CalcResult calcResult, CalculatorFileResult exportResult, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await billingInstructionService.CreateBillingInstructions(runContext, calcResult);
            await producerInvoiceNetTonnageService.CreateProducerInvoiceNetTonnage(runContext, calcResult);
            await SaveExportMetadata(exportResult, cancellationToken);
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

    private async Task SaveExportMetadata(CalculatorFileResult exportResult, CancellationToken cancellationToken)
    {
        dbContext.CalculatorRunCsvFileMetadata.Add(exportResult.CsvMetadata);

        await dbContext.SaveChangesAsync(cancellationToken);
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
