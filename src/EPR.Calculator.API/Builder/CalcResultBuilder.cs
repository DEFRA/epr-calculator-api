using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ApplicationDBContext context;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        public CalcResultBuilder(ApplicationDBContext context, ICalcResultDetailBuilder calcResultDetailBuilder) 
        {
            this.context = context;
            this.calcResultDetailBuilder = calcResultDetailBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            return calcResult;
        }
    }
}
