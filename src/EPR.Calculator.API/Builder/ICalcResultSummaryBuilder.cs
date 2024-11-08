using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public interface ICalcResultSummaryBuilder
    {
        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto);
    }
}
