using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Services;

internal static class ProducerBillingInstructionsQueryExtensions
{
    private const string NoActionPlaceholder = "-";

    public static IQueryable<ProducerBillingInstructionsDto> ApplyOrganisationIdFilter(
        this IQueryable<ProducerBillingInstructionsDto> query,
        ProducerBillingInstructionsSearchQueryDto? searchQuery)
    {
            if (searchQuery?.OrganisationId.HasValue == true)
            {
                var orgId = searchQuery.OrganisationId.Value;
                query = query.Where(x => x.ProducerId == orgId).AsQueryable();
            }

            return query;
    }

    public static IQueryable<ProducerBillingInstructionsDto> ApplyStatusFilter(
        this IQueryable<ProducerBillingInstructionsDto> query,
        ProducerBillingInstructionsSearchQueryDto? searchQuery)
    {
        if (searchQuery?.Status != null && searchQuery.Status.Any())
        {
            var statusList = searchQuery.Status.ToList();
            query = query.Where(x => x.BillingInstructionAcceptReject != null && statusList.Contains(x.BillingInstructionAcceptReject)).AsQueryable();
        }

        return query;
    }

    public static IQueryable<ProducerBillingInstructionsDto> ApplyBillingInstructionFilter(
        this IQueryable<ProducerBillingInstructionsDto> query,
        ProducerBillingInstructionsSearchQueryDto? searchQuery)
    {
        if (searchQuery?.BillingInstruction != null && searchQuery.BillingInstruction.Any())
        {
            var billingInstructionList = searchQuery.BillingInstruction.Select(b => b?.Trim()).Where(b => !string.IsNullOrWhiteSpace(b)).ToList();

            bool includeNoAction = billingInstructionList.Exists(b => string.Equals(b, BillingInstructionAction.Noaction.ToString(), StringComparison.OrdinalIgnoreCase));

            if (includeNoAction)
            {
                query = query.Where(x =>
                    string.IsNullOrWhiteSpace(x.SuggestedBillingInstruction) || x.SuggestedBillingInstruction.Trim() == NoActionPlaceholder
                    || (x.SuggestedBillingInstruction != null && billingInstructionList.Contains(x.SuggestedBillingInstruction.Trim())))
                    .AsQueryable();
            }
            else
            {
                query = query.Where(x => x.SuggestedBillingInstruction != null && billingInstructionList.Contains(x.SuggestedBillingInstruction.Trim())).AsQueryable();
            }
        }

        return query;
    }
}
