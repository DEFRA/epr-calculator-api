using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Services;

public interface IInvoicedProducerService
{
    public Task<ImmutableHashSet<int>> GetProducerIdsForRun(int runId, CancellationToken cancellationToken = default);
    public Task<ImmutableHashSet<int>> GetAcceptedCancelledProducerIdsForRun(int runId, CancellationToken cancellationToken = default);
    public Task<ImmutableHashSet<int>> GetInvoicedThenCancelledProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
    Task<ImmutableHashSet<int>> GetInvoicedProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
    public Task<ImmutableList<InvoicedProducer>> GetInvoicedProducers(RelativeYear relativeYear, ImmutableHashSet<int>? producerIdFilter = null, CancellationToken cancellationToken = default);
    public Task<ImmutableList<InvoicedProducer>> GetLatestAcceptedInvoicedProducers(RelativeYear relativeYear, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage(Justification = "Tests to be re-added within ECV-473")]
public class InvoicedProducerService(
    ApplicationDBContext dbContext,
    ILogger<InvoicedProducerService> logger)
    : IInvoicedProducerService
{
    private static readonly ImmutableHashSet<int> ValidClassifications =
    [
        RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
        RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
        RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
        RunClassificationStatusIds.FINALRUNCOMPLETEDID
    ];

    public async Task<ImmutableHashSet<int>> GetProducerIdsForRun(int runId, CancellationToken cancellationToken = default)
    {
        var query =
            from
                producerDetail in dbContext.ProducerDetail
            where
                producerDetail.CalculatorRunId == runId
            select
                producerDetail.ProducerId;

        return await query.Distinct().ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetAcceptedCancelledProducerIdsForRun(int runId, CancellationToken cancellationToken = default)
    {
        var query =
            from
                suggested in dbContext.ProducerResultFileSuggestedBillingInstruction
            where
                suggested.CalculatorRunId == runId
                && suggested.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                && suggested.SuggestedBillingInstruction == PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill
            select
                suggested.ProducerId;

        return await query.Distinct().ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetInvoicedThenCancelledProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        var query =
            from
                suggested in dbContext.ProducerResultFileSuggestedBillingInstruction
            join
                run in dbContext.CalculatorRuns
                on suggested.CalculatorRunId equals run.Id
            where
                ValidClassifications.Contains(run.CalculatorRunClassificationId)
                && run.RelativeYearValue == relativeYear.Value
                && suggested.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                && suggested.SuggestedBillingInstruction == PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill
            select
                suggested.ProducerId;

        return await query.Distinct().ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableHashSet<int>> GetInvoicedProducerIdsForYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        var query =
            from
                suggested in dbContext.ProducerResultFileSuggestedBillingInstruction
            join
                run in dbContext.CalculatorRuns
                on suggested.CalculatorRunId equals run.Id
            where
                ValidClassifications.Contains(run.CalculatorRunClassificationId)
                && run.RelativeYearValue == relativeYear.Value
                && suggested.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
            select
                suggested.ProducerId;

        return await query.Distinct().ToImmutableHashSetAsync(cancellationToken);
    }

    public async Task<ImmutableList<InvoicedProducer>> GetInvoicedProducers(RelativeYear relativeYear, ImmutableHashSet<int>? producerIdFilter = null, CancellationToken cancellationToken = default)
    {
        if (producerIdFilter is not null)
        {
            if (producerIdFilter.Count == 0)
                return ImmutableList<InvoicedProducer>.Empty;

            if (producerIdFilter.Count > 10000)
            {
                // Not expecting to have much more than ~5,000 entries in the filter.
                // If this warning starts triggering, the performance of this query should be revisited - it may be
                // better to chunk the filter or join on a temp table.
                logger.LogWarning("ProducerIdFilter count is high. Count:{FilterCount}", producerIdFilter.Count);
            }
        }

        var query =
            from
                projection in GetInvoicedProducerProjection()
            where
                ValidClassifications.Contains(projection.CalculatorRun.CalculatorRunClassificationId)
                && projection.CalculatorRun.RelativeYearValue == relativeYear.Value
                && projection.SuggestedInstruction.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                && (producerIdFilter == null || producerIdFilter.Contains(projection.ProducerId))
            select
                new InvoicedProducer
                {
                    CalculatorRunId = projection.CalculatorRun.Id,
                    CalculatorName = projection.CalculatorRun.Name,
                    ProducerId = projection.InvoicedTonnage.ProducerId,
                    ProducerName = projection.OrgDetail.ProducerName,
                    TradingName = projection.OrgDetail.TradingName,
                    MaterialId = projection.InvoicedTonnage.MaterialId,
                    InvoicedNetTonnage = projection.InvoicedTonnage.InvoicedNetTonnage,
                    BillingInstructionId = projection.InvoiceInstruction.BillingInstructionId,
                    CurrentYearInvoicedTotalAfterThisRun = projection.InvoiceInstruction.CurrentYearInvoicedTotalAfterThisRun
                };

        return await query.ToImmutableListAsync(cancellationToken);
    }

    public Task<ImmutableList<InvoicedProducer>> GetLatestAcceptedInvoicedProducers(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        var query =
            from
                projection in GetInvoicedProducerProjection()
            where
                ValidClassifications.Contains(projection.CalculatorRun.CalculatorRunClassificationId)
                && projection.CalculatorRun.RelativeYearValue == relativeYear.Value
                && projection.SuggestedInstruction.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                && projection.SuggestedInstruction.SuggestedBillingInstruction != PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill

                // Exclude rows that have been superseded by a later valid run for this producer, i.e. either:
                //  - a later accepted cancel-bill for the producer (any material), or
                //  - a later accepted non-cancel billing for the same producer + material (i.e. this row isn't the latest).
                && !(
                    from
                        laterRun in dbContext.CalculatorRuns
                    join
                        laterSuggested in dbContext.ProducerResultFileSuggestedBillingInstruction
                        on laterRun.Id equals laterSuggested.CalculatorRunId
                    where
                        laterRun.Id > projection.CalculatorRun.Id
                        && laterRun.RelativeYearValue == relativeYear.Value
                        && ValidClassifications.Contains(laterRun.CalculatorRunClassificationId)
                        && laterSuggested.ProducerId == projection.ProducerId
                        && laterSuggested.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                        && (
                            laterSuggested.SuggestedBillingInstruction == PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill
                            || dbContext.ProducerInvoicedMaterialNetTonnage.Any(t =>
                                t.CalculatorRunId == laterRun.Id
                                && t.ProducerId == projection.InvoicedTonnage.ProducerId
                                && t.MaterialId == projection.InvoicedTonnage.MaterialId)
                        )
                    select
                        1
                ).Any()
            select
                new InvoicedProducer
                {
                    CalculatorRunId = projection.CalculatorRun.Id,
                    CalculatorName = projection.CalculatorRun.Name,
                    ProducerId = projection.InvoicedTonnage.ProducerId,
                    ProducerName = projection.OrgDetail.ProducerName,
                    TradingName = projection.OrgDetail.TradingName,
                    MaterialId = projection.InvoicedTonnage.MaterialId,
                    BillingInstructionId = projection.InvoiceInstruction.BillingInstructionId,
                    InvoicedNetTonnage = projection.InvoicedTonnage.InvoicedNetTonnage,
                    CurrentYearInvoicedTotalAfterThisRun = projection.InvoiceInstruction.CurrentYearInvoicedTotalAfterThisRun
                };

        return query.ToImmutableListAsync(cancellationToken);
    }

    private IQueryable<InvoicedProducerProjection> GetInvoicedProducerProjection()
    {
        return from
                run in dbContext.CalculatorRuns
            join
                suggested in dbContext.ProducerResultFileSuggestedBillingInstruction
                on run.Id equals suggested.CalculatorRunId
            join
                invoicedTonnage in dbContext.ProducerInvoicedMaterialNetTonnage
                on new { suggested.ProducerId, suggested.CalculatorRunId } equals new { invoicedTonnage.ProducerId, invoicedTonnage.CalculatorRunId }
            join
                invoiceInstruction in dbContext.ProducerDesignatedRunInvoiceInstruction
                on new { suggested.ProducerId, suggested.CalculatorRunId } equals new { invoiceInstruction.ProducerId, invoiceInstruction.CalculatorRunId }
            join
                orgDetail in GetPreferredOrgDetailsProjection()
                on suggested.ProducerId equals orgDetail.ProducerId
            select
                new InvoicedProducerProjection
                {
                    ProducerId = suggested.ProducerId,
                    CalculatorRun = run,
                    SuggestedInstruction = suggested,
                    InvoicedTonnage = invoicedTonnage,
                    InvoiceInstruction = invoiceInstruction,
                    OrgDetail = orgDetail
                };
    }

    private IQueryable<InvoicedProducerProjection.PreferredOrgDetail> GetPreferredOrgDetailsProjection()
    {
        var eligible = dbContext.CalculatorRunOrganisationDataDetails
            .Where(detail => string.IsNullOrEmpty(detail.SubsidiaryId));

        return
            from
                eligibleId in (from detail in eligible select detail.OrganisationId).Distinct()
            from preferred in (
                from
                    detail in eligible
                where
                    detail.OrganisationId == eligibleId
                orderby
                    detail.CalculatorRunOrganisationDataMasterId descending,
                    detail.ObligationStatus == ObligationStates.Obligated ? 0 : 1,
                    detail.Id
                select detail).Take(1)
            select new InvoicedProducerProjection.PreferredOrgDetail
            {
                ProducerId = preferred.OrganisationId,
                MasterId = preferred.CalculatorRunOrganisationDataMasterId,
                ProducerName = preferred.OrganisationName,
                TradingName = preferred.TradingName
            };
    }

    // ⚠️ Do not use c# record constructor style, EF cannot translate it to SQL.
    private sealed record InvoicedProducerProjection
    {
        public required int ProducerId { get; init; }
        public required CalculatorRun CalculatorRun { get; init; }
        public required ProducerResultFileSuggestedBillingInstruction SuggestedInstruction { get; init; }
        public required ProducerInvoicedMaterialNetTonnage InvoicedTonnage { get; init; }
        public required ProducerDesignatedRunInvoiceInstruction InvoiceInstruction { get; init; }
        public required PreferredOrgDetail OrgDetail { get; init; }

        public sealed record PreferredOrgDetail
        {
            public required int MasterId { get; init; }
            public required int ProducerId { get; init; }
            public required string ProducerName { get; init; }
            public required string? TradingName { get; init; }
        }
    }
}
