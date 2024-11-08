using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultSummaryBuilder summaryBuilder;

        public CalcResultBuilder(ICalcResultDetailBuilder calcResultDetailBuilder, ICalcResultLapcapDataBuilder lapcapBuilder, ICalcResultSummaryBuilder summaryBuilder) 
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.summaryBuilder = summaryBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            // calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            // calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);

            var result = this.summaryBuilder.Construct(resultsRequestDto);

            return calcResult;
        }
    }
}
