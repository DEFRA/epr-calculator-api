using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
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
            var billingInstructionsQuery =
                from prsbi in dbContext.ProducerResultFileSuggestedBillingInstruction
                join pd in dbContext.ProducerDetail
                    on new { prsbi.ProducerId, prsbi.CalculatorRunId }
                    equals new { pd.ProducerId, pd.CalculatorRunId } into pdGroup
                from pd in pdGroup.DefaultIfEmpty()
                where prsbi.CalculatorRunId == runContext.RunId
                      && prsbi.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                      && !string.IsNullOrWhiteSpace(prsbi.ReasonForRejection)
                      && pd.SubsidiaryId == null
                select new
                {
                    prsbi.ProducerId,
                    pd.ProducerName,
                    pd.TradingName,
                    prsbi.BillingInstructionAcceptReject,
                    prsbi.SuggestedBillingInstruction,
                    prsbi.SuggestedInvoiceAmount,
                    prsbi.CurrentYearInvoiceTotalToDate,
                    prsbi.LastModifiedAcceptReject,
                    prsbi.LastModifiedAcceptRejectBy,
                    prsbi.ReasonForRejection
                };

            var orgDetailsQuery =
                from cr in dbContext.CalculatorRuns
                join org in dbContext.CalculatorRunOrganisations
                    on cr.Id equals org.CalculatorRunId
                join b in billingInstructionsQuery
                    on org.OrganisationId equals b.ProducerId
                where cr.RelativeYear == runContext.RelativeYear
                      && org.OrganisationName != null
                      && org.SubsidiaryId == null
                group cr by org.OrganisationId into g
                select new
                {
                    OrganisationId = g.Key,
                    LatestRunId = g.Max(x => x.Id)
                };

            var rejectedProducersQuery =
                from cr in dbContext.CalculatorRuns
                join org in dbContext.CalculatorRunOrganisations
                    on cr.Id equals org.CalculatorRunId
                join b in billingInstructionsQuery
                    on org.OrganisationId equals b.ProducerId
                join latest in orgDetailsQuery
                    on new { OrgId = org.OrganisationId, cr.Id }
                    equals new { OrgId = latest.OrganisationId, Id = latest.LatestRunId }
                where org.SubsidiaryId == null
                select new CalcResultRejectedProducer
                {
                    RunId = cr.Id,
                    ProducerId = org.OrganisationId,
                    ProducerName = org.OrganisationName,
                    TradingName = org.TradingName ?? "",
                    SuggestedBillingInstruction = b.SuggestedBillingInstruction,
                    SuggestedInvoiceAmount = (b.SuggestedBillingInstruction == BillingConstants.Suggestion.Cancel &&
                                              b.BillingInstructionAcceptReject == BillingConstants.Action.Rejected
                                             ? (b.CurrentYearInvoiceTotalToDate ?? 0m) : (b.SuggestedInvoiceAmount ?? 0m)),
                    InstructionConfirmedDate = b.LastModifiedAcceptReject,
                    InstructionConfirmedBy = b.LastModifiedAcceptRejectBy,
                    ReasonForRejection = b.ReasonForRejection
                };

            return await rejectedProducersQuery.AsNoTracking().Distinct().ToListAsync(cancellationToken);
        }
    }
}
