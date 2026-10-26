using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
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
            // Matched in memory: joining on the nullable subsidiary ID in SQL scans the whole organisation table once per error row.
            var masterId = context.CalculatorRuns
                .AsNoTracking()
                .Where(run => run.Id == runContext.RunId)
                .Select(run => run.CalculatorRunOrganisationDataMasterId)
                .SingleOrDefault();

            if (masterId is null)
                return [];

            var organisations = context.CalculatorRunOrganisationDataDetails
                .AsNoTracking()
                .Where(odd => odd.CalculatorRunOrganisationDataMasterId == masterId)
                .OrderBy(odd => odd.Id)
                .Select(odd => new { odd.OrganisationId, odd.SubsidiaryId, odd.OrganisationName, odd.TradingName })
                .AsEnumerable()
                .GroupBy(odd => (odd.OrganisationId, odd.SubsidiaryId))
                .ToDictionary(g => g.Key, g => g.First());

            return context.ErrorReports
                .AsNoTracking()
                .Where(er => er.CalculatorRunId == runContext.RunId)
                .OrderBy(er => er.Id)
                .AsEnumerable()
                .Select(er =>
                {
                    var organisation = organisations.GetValueOrDefault((er.ProducerId, er.SubsidiaryId));
                    var hasName = organisation is not null && !string.IsNullOrWhiteSpace(organisation.OrganisationName);

                    return new CalcResultErrorReport
                    {
                        Id = er.Id,
                        ProducerId = er.ProducerId,
                        SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,
                        ProducerName = hasName ? organisation!.OrganisationName : CommonConstants.Hyphen,
                        TradingName = hasName ? GetFormatedTradingName(organisation!.TradingName) : CommonConstants.Hyphen,
                        LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                        ErrorCodeText = er.ErrorCode
                    };
                })
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId, x.ErrorCodeText })
                .Select(g => g.First())
                .OrderBy(x => x.ProducerId)
                .ThenBy(x => x.SubsidiaryId)
                .ThenBy(x => x.ErrorCodeText)
                .ToList();
        }

        private static string GetFormatedTradingName(string? tradingName) =>
            string.IsNullOrEmpty(tradingName)
                ? CommonConstants.Hyphen
                : tradingName;
    }
}
