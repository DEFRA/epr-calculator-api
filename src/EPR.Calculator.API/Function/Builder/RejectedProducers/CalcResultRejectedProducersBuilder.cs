using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.RejectedProducers
{
    public interface ICalcResultRejectedProducersBuilder
    {
        public Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }

    public class CalcResultRejectedProducersBuilder : ICalcResultRejectedProducersBuilder
    {
        private readonly ApplicationDBContext dbContext;

        public CalcResultRejectedProducersBuilder(ApplicationDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<CalcResultRejectedProducer>> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var billingInstructionsQuery =
                from prsbi in dbContext.ProducerResultFileSuggestedBillingInstruction
                join pd in dbContext.ProducerDetail
                    on new { prsbi.ProducerId, prsbi.CalculatorRunId }
                    equals new { pd.ProducerId, pd.CalculatorRunId } into pdGroup
                from pd in pdGroup.DefaultIfEmpty()
                where prsbi.CalculatorRunId == resultsRequestDto.RunId
                      && prsbi.BillingInstructionAcceptReject == CommonConstants.Rejected
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
                join crodm in dbContext.CalculatorRunOrganisationDataMaster
                    on cr.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in dbContext.CalculatorRunOrganisationDataDetails
                    on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join b in billingInstructionsQuery
                    on crodd.OrganisationId equals b.ProducerId
                where cr.RelativeYearValue == resultsRequestDto.RelativeYear.Value
                      && crodd.OrganisationName != null
                      && crodd.SubsidiaryId == null
                group cr by crodd.OrganisationId into g
                select new
                {
                    OrganisationId = g.Key,
                    LatestRunId = g.Max(x => x.Id)
                };

            var rejectedProducersQuery =
                from cr in dbContext.CalculatorRuns
                join crodm in dbContext.CalculatorRunOrganisationDataMaster
                    on cr.CalculatorRunOrganisationDataMasterId equals crodm.Id
                join crodd in dbContext.CalculatorRunOrganisationDataDetails
                    on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                join b in billingInstructionsQuery
                    on crodd.OrganisationId equals b.ProducerId
                join latest in orgDetailsQuery
                    on new { OrgId = crodd.OrganisationId, cr.Id }
                    equals new { OrgId = latest.OrganisationId, Id = latest.LatestRunId }
                where crodd.SubsidiaryId == null
                select new CalcResultRejectedProducer
                {
                    runId = cr.Id,
                    ProducerId = crodd.OrganisationId,
                    ProducerName = crodd.OrganisationName,
                    TradingName = crodd.TradingName ?? "",
                    SuggestedBillingInstruction = b.SuggestedBillingInstruction,
                    SuggestedInvoiceAmount = (b.SuggestedBillingInstruction == CommonConstants.CancelStatus &&
                                              b.BillingInstructionAcceptReject == CommonConstants.Rejected
                                             ? (b.CurrentYearInvoiceTotalToDate ?? 0m) : (b.SuggestedInvoiceAmount ?? 0m)),
                    InstructionConfirmedDate = b.LastModifiedAcceptReject,
                    InstructionConfirmedBy = b.LastModifiedAcceptRejectBy,
                    ReasonForRejection = b.ReasonForRejection
                };

            return await rejectedProducersQuery.AsNoTracking().Distinct().ToListAsync();
        }
    }
}
