using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        Task<CalcResultCommsCost> Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment);
    }
}