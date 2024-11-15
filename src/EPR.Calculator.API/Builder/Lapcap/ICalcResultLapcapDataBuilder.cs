using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        public CalcResultLapcapData Construct(CalcResultsRequestDto resultsRequestDto);
    }
    public interface ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}