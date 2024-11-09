using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ParametersOther
{
    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        public CalcResultParameterOtherCost Construct(CalcResultsRequestDto resultsRequestDto)
        {
            return new CalcResultParameterOtherCost();
        }
    }
}
