using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultDetailBuilder : ICalcResultDetailBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultDetailBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultDetail Construct(CalcResultsRequestDto resultsRequestDto)
        {
            return new CalcResultDetail();
        }
    }
}