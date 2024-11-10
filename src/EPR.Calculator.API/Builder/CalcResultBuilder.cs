using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;

        public CalcResultBuilder(ICalcResultDetailBuilder calcResultDetailBuilder, ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder) 
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLateReportingTonnageData = this.lateReportingBuilder.Construct(resultsRequestDto);

            return calcResult;
        }
    }
}
