using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Detail
{
    public interface ICalcResultDetailBuilder
    {
        Task<CalcResultDetail> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }
}