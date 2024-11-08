using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultParamsLateReportingBuilder : ICalcResultParamsLateReportingBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultParamsLateReportingBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultLateReportingTonnage Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var data = new List<CalcResultLateReportingTonnageDetail>();
            return new CalcResultLateReportingTonnage { Name = "Parameters - Late Reporting Tonnages", CalcResultLateReportingTonnageDetails = data };
        }
    }
}