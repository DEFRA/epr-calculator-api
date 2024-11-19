using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder.Detail
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
            var calcResultDetails = context.CalculatorRuns
                .Include(o => o.CalculatorRunOrganisationDataMaster)
                .Include(o => o.CalculatorRunPomDataMaster)
                .Include(o => o.DefaultParameterSettingMaster)
                .Include(x => x.LapcapDataMaster)
                .ToListAsync();

            var results = new CalcResultDetail();
            var calcResultDetail = calcResultDetails.Result.Find(x => x.Id == resultsRequestDto.RunId);
            if (calcResultDetail != null)
            {
                results.RunId = calcResultDetail.Id;
                results.RunName = calcResultDetail.Name;
                results.RunBy = calcResultDetail.CreatedBy;
                results.RunDate = calcResultDetail.CreatedAt;
                results.FinancialYear = calcResultDetail.Financial_Year;
                if (calcResultDetail.CalculatorRunOrganisationDataMaster != null)
                    results.RpdFileORG = calcResultDetail.CalculatorRunOrganisationDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (calcResultDetail.CalculatorRunPomDataMaster != null)
                    results.RpdFilePOM = calcResultDetail.CalculatorRunPomDataMaster.CreatedAt.ToString(CalculationResults.DateFormat);
                if (calcResultDetail.LapcapDataMaster != null)
                    results.LapcapFile = FormatFileData(calcResultDetail.LapcapDataMaster.LapcapFileName, calcResultDetail.LapcapDataMaster.CreatedAt, calcResultDetail.LapcapDataMaster.CreatedBy);
                if (calcResultDetail.DefaultParameterSettingMaster != null)
                    results.ParametersFile = FormatFileData(calcResultDetail.DefaultParameterSettingMaster.ParameterFileName, calcResultDetail.DefaultParameterSettingMaster.CreatedAt, calcResultDetail.DefaultParameterSettingMaster.CreatedBy);
            }
            return results;
        }

        private static string FormatFileData(string fileName, DateTime createdAt, string createdBy)
        {
            return $"{fileName},{createdAt.ToString(CalculationResults.DateFormat)},{createdBy}";
        }
    }
}