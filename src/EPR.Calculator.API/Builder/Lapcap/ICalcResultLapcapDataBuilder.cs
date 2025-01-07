using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> Construct(CalcResultsRequestDto resultsRequestDto);
    }
}