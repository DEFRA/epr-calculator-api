using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.ErrorReport
{
    public interface ICalcResultErrorReportBuilder
    {
        public IEnumerable<CalcResultErrorReport> Construct(CalcResultsRequestDto resultsRequestDto);
    }
}