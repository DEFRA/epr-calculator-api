using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Api.DataApi.Models;

namespace EPR.Calculator.API.BackgroundService.Services;

public interface IErrorReportService
{
    /// <summary>
    ///     Decides which of DataApi's calculated errors/warnings to keep, and persists them as
    ///     <see cref="ErrorReport" /> rows for a calculator run.
    /// </summary>
    /// <remarks>
    ///     DataApi can't see billing history, so any error/warning it raises with no current-year POM
    ///     match (<see cref="ProducerCalculationError.HasPomMatch" /> false) is only kept here if the
    ///     organisation was invoiced in a previous run this financial year - otherwise it's a stale
    ///     status error for a producer with no reason to still appear.
    /// </remarks>
    Task PersistErrors(
        IReadOnlyList<ProducerRecord> producerRecords,
        int calculatorRunId,
        string createdBy,
        RelativeYear relativeYear,
        CancellationToken cancellationToken);
}

public class ErrorReportService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IInvoicedProducerService invoicedProducerService)
    : IErrorReportService
{
    public async Task PersistErrors(
        IReadOnlyList<ProducerRecord> producerRecords,
        int calculatorRunId,
        string createdBy,
        RelativeYear relativeYear,
        CancellationToken cancellationToken)
    {
        var invoicedProducers = await invoicedProducerService.GetInvoicedProducers(relativeYear, cancellationToken: cancellationToken);
        var invoicedOrganisationIds = invoicedProducers.Select(i => i.ProducerId).ToHashSet();

        var errors = producerRecords
            .SelectMany(record => record.Errors.Concat(record.Warnings)
                .Select(error =>
                (
                    OrganisationId: record.OrganisationId,
                    SubsidiaryId: record.SubsidiaryId,
                    Error: error
                )))
            .ToList();

        var createdAt = DateTime.UtcNow;

        var reports = errors
            .Where(e => e.Error.HasPomMatch || invoicedOrganisationIds.Contains(e.OrganisationId))
            .Select(e => new ErrorReport
            {
                CalculatorRunId = calculatorRunId,
                ProducerId = e.OrganisationId,
                SubsidiaryId = e.SubsidiaryId,
                ErrorCode = e.Error.ErrorCode,
                LeaverCode = e.Error.LeaverCode,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            })
            .ToList();

        await bulkOps.BulkInsertAsync(dbContext, reports, cancellationToken);
    }
}
