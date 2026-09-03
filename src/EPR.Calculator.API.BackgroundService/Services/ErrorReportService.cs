using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.CommonDataService.DataApi.Alignment;

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
    ///     status error for a producer with no reason to still appear. A holding-company-level roll-up
    ///     is then added for any producer whose surviving errors are all subsidiary-scoped.
    /// </remarks>
    Task PersistErrors(
        IReadOnlyList<ProducerCalculationError> errors,
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
        IReadOnlyList<ProducerCalculationError> errors,
        int calculatorRunId,
        string createdBy,
        RelativeYear relativeYear,
        CancellationToken cancellationToken)
    {
        var invoicedProducers = await invoicedProducerService.GetInvoicedProducers(relativeYear, cancellationToken: cancellationToken);
        var invoicedOrganisationIds = invoicedProducers.Select(i => i.ProducerId).ToHashSet();

        var displayedErrors = errors
            .Where(e => e.HasPomMatch || invoicedOrganisationIds.Contains(e.OrganisationId))
            .ToImmutableList();

        // Roll up a holding-company-level error for any producer whose surviving errors are all
        // subsidiary-scoped, so the holding company itself also shows up in the error report.
        var holdingRegErrors = displayedErrors
            .GroupBy(x => x.OrganisationId)
            .Where(x => !x.Any(y => string.IsNullOrEmpty(y.SubsidiaryId)))
            .Select(x => new ProducerCalculationError
            {
                OrganisationId = x.Key,
                SubsidiaryId = null,
                ErrorCode = ProducerErrorCodes.Empty,
                LeaverCode = ProducerErrorCodes.Empty,
                IsWarning = false,
                HasPomMatch = true // Irrelevant here - the roll-up isn't itself filtered by HasPomMatch.
            })
            .ToImmutableList();

        var createdAt = DateTime.UtcNow;

        var reports = displayedErrors
            .Concat(holdingRegErrors)
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
