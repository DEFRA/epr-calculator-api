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
            // Matched in memory: joining on the nullable subsidiary ID in SQL scans the organisation table once per error row.
            var organisations = context.CalculatorRunOrganisations
                .AsNoTracking()
                .Where(o => o.CalculatorRunId == runContext.RunId)
                .OrderBy(o => o.Id)
                .AsEnumerable()
                .GroupBy(o => (o.OrganisationId, o.SubsidiaryId))
                .ToDictionary(g => g.Key, g => g.First());

            var errorReports = context.ErrorReports
                .AsNoTracking()
                .Where(er => er.CalculatorRunId == runContext.RunId)
                .OrderBy(er => er.Id)
                .AsEnumerable()
                .Select(er =>
                {
                    var (producerName, tradingName) = GetNames(organisations.GetValueOrDefault((er.ProducerId, er.SubsidiaryId)));
                    return new CalcResultErrorReport
                    {
                        Id = er.Id,
                        ProducerId = er.ProducerId,
                        SubsidiaryId = er.SubsidiaryId ?? CommonConstants.Hyphen,
                        ProducerName = producerName,
                        TradingName = tradingName,
                        LeaverCode = er.LeaverCode ?? CommonConstants.Hyphen,
                        ErrorCodeText = er.ErrorCode
                    };
                })
                .GroupBy(x => new { x.ProducerId, x.SubsidiaryId, x.ErrorCodeText })
                .Select(g => g.First())
                .ToList();

            return errorReports
                .Concat(HoldingCompanyRows(errorReports, organisations))
                .OrderBy(x => x.ProducerId)
                .ThenBy(x => x.SubsidiaryId)
                .ThenBy(x => x.ErrorCodeText)
                .ToList();
        }

        /// <summary>
        ///     A blank holding-company-level row for any producer whose errors are all subsidiary-scoped,
        ///     so the holding company itself also shows up in the error report.
        /// </summary>
        private static List<CalcResultErrorReport> HoldingCompanyRows(
            IReadOnlyList<CalcResultErrorReport> errorReports,
            Dictionary<(int OrganisationId, string? SubsidiaryId), CalculatorRunOrganisation> organisations)
        {
            return errorReports
                .GroupBy(x => x.ProducerId)
                .Where(g => g.All(x => x.SubsidiaryId != CommonConstants.Hyphen))
                .Select(g =>
                {
                    var (producerName, tradingName) = GetNames(organisations.GetValueOrDefault((g.Key, null)));
                    return new CalcResultErrorReport
                    {
                        ProducerId = g.Key,
                        SubsidiaryId = CommonConstants.Hyphen,
                        ProducerName = producerName,
                        TradingName = tradingName,
                        LeaverCode = string.Empty,
                        ErrorCodeText = string.Empty
                    };
                })
                .ToList();
        }

        private static (string ProducerName, string TradingName) GetNames(CalculatorRunOrganisation? organisation) =>
            organisation is not null && !string.IsNullOrWhiteSpace(organisation.OrganisationName)
                ? (organisation.OrganisationName, GetFormatedTradingName(organisation.TradingName))
                : (CommonConstants.Hyphen, CommonConstants.Hyphen);

        private static string GetFormatedTradingName(string? tradingName) =>
            string.IsNullOrEmpty(tradingName)
                ? CommonConstants.Hyphen
                : tradingName;
    }
}
