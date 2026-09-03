using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IErrorReportService
{
    /// <summary>
    ///     Persists DataApi's calculated errors/warnings as <see cref="ErrorReport" /> rows for a
    ///     calculator run.
    /// </summary>
    Task PersistErrors(
        IReadOnlyList<ProducerCalculationError> errors,
        int calculatorRunId,
        string createdBy,
        CancellationToken cancellationToken);
}

public class ErrorReportService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps)
    : IErrorReportService
{
    public async Task PersistErrors(
        IReadOnlyList<ProducerCalculationError> errors,
        int calculatorRunId,
        string createdBy,
        CancellationToken cancellationToken)
    {
        var createdAt = DateTime.UtcNow;

        var reports = errors
            .Select(e => new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = e.OrganisationId,
                SubsidiaryId = e.SubsidiaryId,
                ErrorCode = e.ErrorCode,
                LeaverCode = e.LeaverCode,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            })
            .ToList();

        await bulkOps.BulkInsertAsync(dbContext, reports, cancellationToken);
    }
}
