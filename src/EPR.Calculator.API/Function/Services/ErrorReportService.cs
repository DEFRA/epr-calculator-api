using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services;

public interface IErrorReportService
{
    Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
        IReadOnlyList<CalculatorRunPomDataDetail> pomDetails,
        IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails,
        int calculatorRunId,
        string createdBy,
        RelativeYear relativeYear,
        CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage(Justification = "Tests to be re-added within ECV-473")]
public class ErrorReportService(
    ApplicationDBContext dbContext,
    IBulkOperations bulkOps,
    IInvoicedProducerService invoicedProducerService)
    : IErrorReportService
{
    public async Task<HashSet<(int OrgId, string? SubId)>> HandleErrors(
        IReadOnlyList<CalculatorRunPomDataDetail> pomDetails,
        IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails,
        int calculatorRunId,
        string createdBy,
        RelativeYear relativeYear,
        CancellationToken cancellationToken)
    {
        var invoiced = await invoicedProducerService.GetInvoicedProducers(relativeYear, cancellationToken: cancellationToken);
        var obligatedErrors = HandleObligatedErrors(pomDetails, orgDetails, invoiced, calculatorRunId, createdBy);
        var obligatedWarnings = HandleObligatedWarnings(pomDetails, orgDetails, invoiced, calculatorRunId, createdBy);
        var missingRegErrors = HandleMissingRegistrationData(pomDetails, orgDetails, calculatorRunId, createdBy);
        var missingPomErrors = HandleMissingPomData(pomDetails, orgDetails, calculatorRunId, createdBy);

        var calcErrors = obligatedErrors
            .Concat(missingRegErrors)
            .Concat(obligatedWarnings)
            .Concat(missingPomErrors)
            .ToImmutableList();

        var holdingRegErrors = calcErrors
            .GroupBy(x => x.ProducerId)
            .Where(x => !x.Any(y => string.IsNullOrEmpty(y.SubsidiaryId)))
            .Select(x => CreateError(x.Key, null, calculatorRunId, createdBy, ErrorCodes.Empty, null))
            .ToImmutableList();

        var allErrors = calcErrors.Concat(holdingRegErrors).ToImmutableList();
        await bulkOps.BulkInsertAsync(dbContext, allErrors, cancellationToken);

        return calcErrors
            .Where(e => !obligatedWarnings.Contains(e)) // Filter out warnings so they are kept in calculator results.
            .Select(e => (e.ProducerId, e.SubsidiaryId))
            .ToHashSet();
    }

    public static List<ErrorReport> HandleMissingRegistrationData(
        IReadOnlyList<CalculatorRunPomDataDetail> pomDetails,
        IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails,
        int calculatorRunId,
        string createdBy)
    {
        return pomDetails
            .DistinctBy(x => (x.OrganisationId, x.SubsidiaryId, x.SubmitterId))
            .GroupBy(x => x.OrganisationId)
            .SelectMany(group =>
            {
                var reg = orgDetails.Where(p => p.OrganisationId == group.Key);
                var missing = group.Any(o => !reg.Any(p => p.SubsidiaryId == o.SubsidiaryId && p.SubmitterId == o.SubmitterId));

                return missing
                    ? group.Select(x => CreateError(x.OrganisationId ?? 0, x.SubsidiaryId, calculatorRunId, createdBy, ErrorCodes.MissingRegistrationData, null))
                    : Enumerable.Empty<ErrorReport>();
            })
            .ToList();
    }

    public static List<ErrorReport> HandleMissingPomData(IReadOnlyList<CalculatorRunPomDataDetail> pomDetails, IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails, int calculatorRunId, string createdBy)
    {
        // Pre-compute the set of POM keys (subsidiary id, falling back to org id) so the
        // membership check below is O(1) per orgDetail rather than O(P) per orgDetail.
        // This drops the overall cost from O(O*P) to O(O+P), which matters for large runs.
        var pomKeys = new HashSet<string>(pomDetails.Count, StringComparer.Ordinal);
        foreach (var p in pomDetails)
        {
            var key = p.SubsidiaryId ?? p.OrganisationId.ToString()!;
            pomKeys.Add(key);
        }

        return orgDetails
            .Where(o => ObligationStates.IsObligated(o.ObligationStatus))
            // Only raise errors for missing POM when they previously had POM data submitted to avoid loads of errors
            .Where(o => pomKeys.Contains(o.SubsidiaryId ?? o.OrganisationId.ToString()))
            .Where(reg => reg is not { HasH1: true, HasH2: true })
            .Select(reg => CreateError(reg.OrganisationId, reg.SubsidiaryId, calculatorRunId, createdBy, ErrorCodes.MissingPOMData, reg.StatusCode))
            .ToList();
    }

    public static List<ErrorReport> HandleObligatedErrors(IReadOnlyList<CalculatorRunPomDataDetail> pomDetails, IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails, IReadOnlyList<InvoicedProducer> invoicedDetailsForFY, int calculatorRunId, string createdBy)
    {
        return orgDetails
            .Where(x => x.ObligationStatus == ObligationStates.Error)
            .Where(o =>
                pomDetails.Any(p => new { OrgId = p.OrganisationId, p.SubsidiaryId, p.SubmitterId }.Equals(new { OrgId = (int?)o.OrganisationId, o.SubsidiaryId, o.SubmitterId }))
                || invoicedDetailsForFY.Any(i => i.ProducerId == o.OrganisationId)
            )
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, calculatorRunId, createdBy, x.ErrorCode, x.StatusCode))
            .ToList();
    }


    public static List<ErrorReport> HandleObligatedWarnings(IReadOnlyList<CalculatorRunPomDataDetail> pomDetails, IReadOnlyList<CalculatorRunOrganisationDataDetail> orgDetails, IReadOnlyList<InvoicedProducer> invoicedDetailsForFY, int calculatorRunId, string createdBy)
    {
        return orgDetails
            .Where(x => x.ObligationStatus == ObligationStates.Obligated && !string.IsNullOrEmpty(x.ErrorCode))
            .Where(o =>
                pomDetails.Any(p => new { OrgId = p.OrganisationId, p.SubsidiaryId, p.SubmitterId }.Equals(new { OrgId = (int?)o.OrganisationId, o.SubsidiaryId, o.SubmitterId }))
                || invoicedDetailsForFY.Any(i => i.ProducerId == o.OrganisationId)
            )
            .Select(x => CreateError(x.OrganisationId, x.SubsidiaryId, calculatorRunId, createdBy, x.ErrorCode, x.StatusCode))
            .ToList();
    }

    private static ErrorReport CreateError(int orgId, string? subId, int calculatorRunId, string createdBy, string? errorCode, string? leaverCode)
    {
        return new ErrorReport
        {
            CalculatorRunId = calculatorRunId,
            ProducerId = orgId,
            SubsidiaryId = subId,
            ErrorCode = errorCode ?? string.Empty,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            LeaverCode = leaverCode ?? string.Empty
        };
    }
}
