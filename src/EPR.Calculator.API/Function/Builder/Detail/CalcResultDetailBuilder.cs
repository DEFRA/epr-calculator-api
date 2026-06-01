using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.Detail
{
    public class CalcResultDetailBuilder : ICalcResultDetailBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultDetailBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultDetail> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            var calculatorRuns = await context.CalculatorRuns
                .Include(o => o.CalculatorRunOrganisationDataMaster)
                .Include(o => o.CalculatorRunPomDataMaster)
                .Include(o => o.DefaultParameterSettingMaster)
                .Include(x => x.LapcapDataMaster)
                .ToListAsync();

            string idMsg() => $"CalculatorRun {resultsRequestDto.RunId}";

            var calculatorRun = calculatorRuns.Find(x => x.Id == resultsRequestDto.RunId)
                                ?? throw new InvalidOperationException($"{idMsg()}  not found.");

            var results = new CalcResultDetail
            {
                RunId = calculatorRun.Id,
                RunName = calculatorRun.Name ?? throw new InvalidOperationException($"{idMsg()} has no Name assigned."),
                RunBy = calculatorRun.CreatedBy ?? throw new InvalidOperationException($"{idMsg()} has no CreatedBy assigned."),
                RunDate = calculatorRun.CreatedAt,
                RelativeYear = calculatorRun.RelativeYear ?? throw new InvalidOperationException($"{idMsg()} has no RelativeYear assigned."),
                RpdFileORG = calculatorRun.CalculatorRunOrganisationDataMaster != null
                                ? calculatorRun.CalculatorRunOrganisationDataMaster.CreatedAt.ToString(CalculationResults.DateFormat)
                                : string.Empty,
                RpdFilePOM = calculatorRun.CalculatorRunPomDataMaster != null
                                ? calculatorRun.CalculatorRunPomDataMaster.CreatedAt.ToString(CalculationResults.DateFormat)
                                : string.Empty,
                LapcapFile = calculatorRun.LapcapDataMaster != null
                                ? FormatFileData(
                                    calculatorRun.LapcapDataMaster.LapcapFileName,
                                    calculatorRun.LapcapDataMaster.CreatedAt,
                                    calculatorRun.LapcapDataMaster.CreatedBy)
                                : string.Empty,
                ParametersFile = calculatorRun.DefaultParameterSettingMaster != null
                                    ? FormatFileData(
                                        calculatorRun.DefaultParameterSettingMaster.ParameterFileName,
                                        calculatorRun.DefaultParameterSettingMaster.CreatedAt,
                                        calculatorRun.DefaultParameterSettingMaster.CreatedBy)
                                    : string.Empty
            };

            return results;
        }

        private static string FormatFileData(string fileName, DateTime createdAt, string createdBy)
        {
            return $"{fileName},{createdAt.ToString(CalculationResults.DateFormat)},{createdBy}";
        }
    }
}