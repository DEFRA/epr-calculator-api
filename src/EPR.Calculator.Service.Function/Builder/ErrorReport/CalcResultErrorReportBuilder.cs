using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ErrorReport
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

        public IEnumerable<CalcResultErrorReport> Construct(RunContext runContext)
        {
            var baseQuery =
                from run in context.CalculatorRuns
                where run.Id == runContext.RunId

                join er in context.ErrorReports on run.Id equals er.CalculatorRunId
                join odm in context.CalculatorRunOrganisationDataMaster
                    on run.CalculatorRunOrganisationDataMasterId equals odm.Id

                // LEFT JOIN to find a subsidiary-specific detail: match ProdId + SubsId
                join subOdd in context.CalculatorRunOrganisationDataDetails
                    on new { OrgId = er.ProducerId, MasterId = odm.Id, SubsId = er.SubsidiaryId }
                    equals new { OrgId = subOdd.OrganisationId, MasterId = subOdd.CalculatorRunOrganisationDataMasterId, SubsId = subOdd.SubsidiaryId }
                    into subGroup
                from subLeft in subGroup.DefaultIfEmpty()

                    // LEFT JOIN to find a producer-level detail (SubsidiaryId null) as fallback
                join prodOdd in context.CalculatorRunOrganisationDataDetails
                    on new { OrgId = er.ProducerId, MasterId = odm.Id, SubsId = (string?)null }
                    equals new { OrgId = prodOdd.OrganisationId, MasterId = prodOdd.CalculatorRunOrganisationDataMasterId, SubsId = prodOdd.SubsidiaryId }
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

        private static string GetProducerName(CalculatorRunOrganisationDataDetail prodLeft)
        {
            if(prodLeft != null && !string.IsNullOrWhiteSpace(prodLeft.OrganisationName))
            {
               return prodLeft.SubsidiaryId == null ? CommonConstants.Hyphen : prodLeft.OrganisationName;
            }
            return CommonConstants.Hyphen;
        }

        private static string GetTradingName(CalculatorRunOrganisationDataDetail prodLeft)
        {
            if (prodLeft != null && !string.IsNullOrWhiteSpace(prodLeft.OrganisationName))
            {
                return (prodLeft.SubsidiaryId == null || prodLeft.TradingName is null) ? CommonConstants.Hyphen :
                    GetFormatedTradingName(prodLeft.TradingName);
            }
            return CommonConstants.Hyphen;
        }

        private static string GetFormatedTradingName(string? tradingName)
        {
            return string.IsNullOrEmpty(tradingName) ? CommonConstants.Hyphen : tradingName;
        }

        private static bool IsSubsidary(CalculatorRunOrganisationDataDetail subLeft)
        {
            return (subLeft != null && !string.IsNullOrWhiteSpace(subLeft.OrganisationName));
        }
    }
}
