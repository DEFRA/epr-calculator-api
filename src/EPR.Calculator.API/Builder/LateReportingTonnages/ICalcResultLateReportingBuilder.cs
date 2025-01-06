using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.LateReportingTonnages
{
    public interface ICalcResultLateReportingBuilder
    {
        public Task<CalcResultLateReportingTonnage> Construct(CalcResultsRequestDto resultsRequestDto);
    }
}