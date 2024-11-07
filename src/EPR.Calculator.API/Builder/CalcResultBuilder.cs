using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        public CalcResultBuilder(ICalcResultDetailBuilder calcResultDetailBuilder, ICalcResultLapcapDataBuilder lapcapBuilder) 
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);

            return calcResult;
        }
    }
}
