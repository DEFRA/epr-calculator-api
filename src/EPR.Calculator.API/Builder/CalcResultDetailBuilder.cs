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
                    results.RpdFileORG = item.CalculatorRunOrganisationDataMaster.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                if (item.CalculatorRunPomDataMaster != null)
                    results.RpdFilePOM = item.CalculatorRunPomDataMaster.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                if (item.LapcapDataMaster != null)
                    results.LapcapFile = "TestFileName1" + "," + item.LapcapDataMaster.CreatedAt.ToString("dd/MM/yyyy HH:mm") + "," + item.LapcapDataMaster.CreatedBy;
                if (item.DefaultParameterSettingMaster != null)
                    results.ParametersFile = item.DefaultParameterSettingMaster.ParameterFileName + "," + item.DefaultParameterSettingMaster.CreatedAt.ToString("dd/MM/yyyy HH:mm") + "," + item.DefaultParameterSettingMaster.CreatedBy;
            }
            return results;
        }
    }
}