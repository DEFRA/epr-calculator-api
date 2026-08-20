using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Builder.Detail
{
    public interface ICalcResultDetailBuilder
    {
        Task<CalcResultDetail> ConstructAsync(RunContext runContext, CancellationToken cancellationToken);
    }

    public class CalcResultDetailBuilder(ApplicationDBContext dbContext)
        : ICalcResultDetailBuilder
    {
        [ActivityTrace]
        public async Task<CalcResultDetail> ConstructAsync(RunContext runContext, CancellationToken cancellationToken)
        {
            var calculatorRun = await dbContext.CalculatorRuns
                .Include(o => o.CalculatorRunOrganisationDataMaster)
                .Include(o => o.CalculatorRunPomDataMaster)
                .Include(o => o.DefaultParameterSettingMaster)
                .Include(x => x.LapcapDataMaster)
                .SingleAsync(x => x.Id == runContext.RunId, cancellationToken: cancellationToken);

            var results = new CalcResultDetail
            {
                RunId = calculatorRun.Id,
                RunName = calculatorRun.Name,
                RunBy = calculatorRun.CreatedBy,
                RunDate = calculatorRun.CreatedAt,
                RelativeYear = calculatorRun.RelativeYear,
                CutOffDate = runContext.DefaultParameters.CutOffDate,
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
