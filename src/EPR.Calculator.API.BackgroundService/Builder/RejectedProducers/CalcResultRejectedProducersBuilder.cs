using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Builder.RejectedProducers
{
    public interface ICalcResultRejectedProducersBuilder
    {
        public Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(RunContext runContext, CancellationToken cancellationToken);
    }

    public class CalcResultRejectedProducersBuilder : ICalcResultRejectedProducersBuilder
    {
        private readonly ApplicationDBContext dbContext;

        public CalcResultRejectedProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [ActivityTrace]
        public async Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(RunContext runContext, CancellationToken cancellationToken)
        {
            var rejectedInstructions = await (
                from prsbi in dbContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                where prsbi.CalculatorRunId == runContext.RunId
                      && prsbi.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                      && !string.IsNullOrWhiteSpace(prsbi.ReasonForRejection)
                select prsbi
            ).ToListAsync(cancellationToken);

            if (rejectedInstructions.Count == 0)
                return [];

            var cancelledProducerIds = rejectedInstructions
                .Where(i => i.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel)
                .Select(i => i.ProducerId)
                .ToHashSet();

            var billedProducerIds = rejectedInstructions
                .Where(i => i.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel)
                .Select(i => i.ProducerId)
                .ToHashSet();

            var billedDetails = await GetCurrentRunDetails(runContext, billedProducerIds, cancellationToken);
            var cancelledDetails = await GetLatestInvoicedDetails(runContext, cancelledProducerIds, cancellationToken);

            return rejectedInstructions
                .Select(instruction =>
                {
                    var details = instruction.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel
                        ? cancelledDetails.GetValueOrDefault(instruction.ProducerId)
                        : billedDetails.GetValueOrDefault(instruction.ProducerId);

                    return new CalcResultRejectedProducer
                    {
                        RunId = runContext.RunId,
                        ProducerId = instruction.ProducerId,
                        ProducerName = details?.Name ?? string.Empty,
                        TradingName = details?.TradingName ?? string.Empty,
                        SuggestedBillingInstruction = instruction.SuggestedBillingInstruction,
                        SuggestedInvoiceAmount = instruction.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel
                            ? instruction.CurrentYearInvoiceTotalToDate ?? 0m
                            : instruction.SuggestedInvoiceAmount ?? 0m,
                        InstructionConfirmedDate = instruction.LastModifiedAcceptReject,
                        InstructionConfirmedBy = instruction.LastModifiedAcceptRejectBy ?? string.Empty,
                        ReasonForRejection = instruction.ReasonForRejection ?? string.Empty
                    };
                })
                .OrderBy(r => r.ProducerId)
                .ToList();
        }

        /// <summary>
        ///     For producers being billed in this run: their holding company producer details, falling back to their
        ///     obligated holding company organisation details for holding companies with no producer details of their own.
        /// </summary>
        private async Task<Dictionary<int, ProducerNameDetails>> GetCurrentRunDetails(
            RunContext runContext,
            IReadOnlyCollection<int> producerIds,
            CancellationToken cancellationToken)
        {
            if (producerIds.Count == 0)
                return [];

            var details = (await dbContext.ProducerDetail
                    .AsNoTracking()
                    .Where(pd => pd.CalculatorRunId == runContext.RunId
                                 && pd.SubsidiaryId == null
                                 && producerIds.Contains(pd.ProducerId))
                    .ToListAsync(cancellationToken))
                .GroupBy(pd => pd.ProducerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(pd => pd.Id).Select(pd => new ProducerNameDetails(pd.ProducerName, pd.TradingName)).First());

            var withoutProducerDetails = producerIds.Where(id => !details.ContainsKey(id)).ToList();

            var organisations = await GetObligatedHoldingCompanyDetails(
                withoutProducerDetails.ToDictionary(id => id, _ => runContext.RunId),
                cancellationToken);

            foreach (var (producerId, organisation) in organisations)
                details[producerId] = organisation;

            return details;
        }

        /// <summary>
        ///     For rejected cancellations: the obligated holding company organisation details from the latest completed
        ///     run before the current one in which the producer was invoiced (an accepted, non-cancel billing instruction).
        /// </summary>
        private async Task<Dictionary<int, ProducerNameDetails>> GetLatestInvoicedDetails(
            RunContext runContext,
            IReadOnlyCollection<int> producerIds,
            CancellationToken cancellationToken)
        {
            if (producerIds.Count == 0)
                return [];

            var latestInvoicedRunByProducer = await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join prsbi in dbContext.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                    on run.Id equals prsbi.CalculatorRunId
                where RunClassificationHelper.CompletedClassifications.Contains(run.Classification)
                      && run.RelativeYear == runContext.RelativeYear
                      && run.Id < runContext.RunId
                      && producerIds.Contains(prsbi.ProducerId)
                      && prsbi.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                      && prsbi.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel
                group run.Id by prsbi.ProducerId into g
                select new { ProducerId = g.Key, RunId = g.Max() }
            ).ToDictionaryAsync(x => x.ProducerId, x => x.RunId, cancellationToken);

            return await GetObligatedHoldingCompanyDetails(latestInvoicedRunByProducer, cancellationToken);
        }

        /// <summary>
        ///     Each producer's obligated holding company organisation details in the given run - the lowest row if there's
        ///     more than one.
        /// </summary>
        private async Task<Dictionary<int, ProducerNameDetails>> GetObligatedHoldingCompanyDetails(
            IReadOnlyDictionary<int, int> runIdByProducer,
            CancellationToken cancellationToken)
        {
            if (runIdByProducer.Count == 0)
                return [];

            var producerIds = runIdByProducer.Keys.ToList();
            var runIds = runIdByProducer.Values.Distinct().ToList();

            var organisations = await (
                from run in dbContext.CalculatorRuns.AsNoTracking()
                join crodd in dbContext.CalculatorRunOrganisationDataDetails.AsNoTracking()
                    on run.CalculatorRunOrganisationDataMasterId equals crodd.CalculatorRunOrganisationDataMasterId
                where runIds.Contains(run.Id)
                      && producerIds.Contains(crodd.OrganisationId)
                      && crodd.SubsidiaryId == null
                      && crodd.ObligationStatus == ObligationStates.Obligated
                select new { RunId = run.Id, crodd.Id, crodd.OrganisationId, crodd.OrganisationName, crodd.TradingName }
            ).ToListAsync(cancellationToken);

            return organisations
                .Where(o => runIdByProducer[o.OrganisationId] == o.RunId)
                .GroupBy(o => o.OrganisationId)
                .Select(g => g.OrderBy(o => o.Id).First())
                .ToDictionary(
                    o => o.OrganisationId,
                    o => new ProducerNameDetails(o.OrganisationName, o.TradingName));
        }

        private sealed record ProducerNameDetails(string? Name, string? TradingName);
    }
}
