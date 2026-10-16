using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Builder.ErrorReport
{
    public interface ICalcResultErrorReportBuilder
    {
        public IEnumerable<CalcResultErrorReport> Construct(RunContext runContext);
    }

    public class CalcResultErrorReportBuilder : ICalcResultErrorReportBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultErrorReportBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        [ActivityTrace]
        public IEnumerable<CalcResultErrorReport> Construct(RunContext runContext)
        {
            var baseQuery =
                from run in context.CalculatorRuns
                where run.Id == runContext.RunId

                join er in context.ErrorReports on run.Id equals er.CalculatorRunId

                // LEFT JOIN to find a subsidiary-specific organisation record: match ProdId + SubsId
                join subOrg in context.CalculatorRunOrganisations
                    on new { OrgId = er.ProducerId, RunId = run.Id, SubsId = er.SubsidiaryId }
                    equals new { OrgId = subOrg.OrganisationId, RunId = subOrg.CalculatorRunId, SubsId = subOrg.SubsidiaryId }
                    into subGroup
                from subLeft in subGroup.DefaultIfEmpty()

                    // LEFT JOIN to find a producer-level organisation record (SubsidiaryId null) as fallback
                join prodOrg in context.CalculatorRunOrganisations
                    on new { OrgId = er.ProducerId, RunId = run.Id, SubsId = (string?)null }
                    equals new { OrgId = prodOrg.OrganisationId, RunId = prodOrg.CalculatorRunId, SubsId = prodOrg.SubsidiaryId }
                    into prodGroup
                from prodLeft in prodGroup.DefaultIfEmpty()

                select new CalcResultErrorReport
                {
                    Id = er.Id,
                    ProducerId = er.ProducerId,
                    SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,

                    // prefer subsidiary-specific name, otherwise producer-level name, otherwise hyphen
                    ProducerName = IsSubsidary(subLeft) ? subLeft.OrganisationName : GetProducerName(prodLeft),

                    TradingName = IsSubsidary(subLeft) ? GetFormatedTradingName(subLeft.TradingName)
                                    : GetTradingName(prodLeft),

                    LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                    ErrorCodeText = er.ErrorCode
                };

            var results = baseQuery
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId, x.ErrorCodeText })
                .Select(g => g.First())
                .OrderBy(x => x.ProducerId)
                .ThenBy(x => x.SubsidiaryId)
                .ThenBy(x => x.ErrorCodeText)
                .ToList();

            return results;
        }

        private static string GetProducerName(CalculatorRunOrganisation? prodLeft) =>
            prodLeft is null || string.IsNullOrWhiteSpace(prodLeft.OrganisationName) || prodLeft.SubsidiaryId == null
                ? CommonConstants.Hyphen
                : prodLeft.OrganisationName;

        private static string GetTradingName(CalculatorRunOrganisation? prodLeft) =>
            prodLeft is null || string.IsNullOrWhiteSpace(prodLeft.OrganisationName) || prodLeft.SubsidiaryId == null
                ? CommonConstants.Hyphen
                : GetFormatedTradingName(prodLeft.TradingName);

        private static string GetFormatedTradingName(string? tradingName) =>
            string.IsNullOrEmpty(tradingName)
                ? CommonConstants.Hyphen
                : tradingName;

        private static bool IsSubsidary(CalculatorRunOrganisation? subLeft) =>
            subLeft != null && !string.IsNullOrWhiteSpace(subLeft.OrganisationName);
    }
}
