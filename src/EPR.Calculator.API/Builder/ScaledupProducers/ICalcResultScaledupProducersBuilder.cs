using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public interface ICalcResultScaledupProducersBuilder
    {
        Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult, IEnumerable<ScaledupProducer> scaledupProducers);
    }
}
