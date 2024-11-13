using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.LaDisposalCost
{
    public interface ICalcRunLaDisposalCostBuilder
    {
        public CalcResultLaDisposalCostData Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult);
    }
}
