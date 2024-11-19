using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        CalcResultCommsCost Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment);
    }
}