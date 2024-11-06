using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultDetailBuilder : ICalcResultDetailBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultDetailBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultDetail Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResultDetail = context.CalculatorRuns
                .Include(o => o.CalculatorRunOrganisationDataMaster)
                .Include(o => o.CalculatorRunPomDataMaster)
                .Include(o => o.DefaultParameterSettingMaster)
                .Include(x => x.LapcapDataMaster)
                .ToListAsync();

            var results = new CalcResultDetail();

            foreach (var item in calcResultDetail.Result)
            {
                results.RunId = item.Id;
                results.RunName = item.Name;
                results.RunBy = item.CreatedBy;
                results.RunDate = item.CreatedAt;
                results.FinancialYear = item.Financial_Year;
                if (item.CalculatorRunOrganisationDataMaster != null)
                    results.RpdFileORG = item.CalculatorRunOrganisationDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (item.CalculatorRunPomDataMaster != null)
                    results.RpdFilePOM = item.CalculatorRunPomDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (item.LapcapDataMaster != null)
                    results.LapcapFile = FormatFileData(item.LapcapDataMaster.LapcapFileName, item.LapcapDataMaster.CreatedAt, item.LapcapDataMaster.CreatedBy);
                if (item.DefaultParameterSettingMaster != null)
                    results.ParametersFile = FormatFileData(item.DefaultParameterSettingMaster.ParameterFileName, item.DefaultParameterSettingMaster.CreatedAt, item.DefaultParameterSettingMaster.CreatedBy);
            }
            return results;
        }

        private static string FormatFileData(string fileName, DateTime createdAt, string createdBy)
        {
            return $"{fileName},{createdAt.ToString(CalculationResults.DateFormat)},{createdBy}";
        }
    }
}