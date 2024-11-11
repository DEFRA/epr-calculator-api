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
        private readonly ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder;

        public CalcResultBuilder(ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder)
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.lapcapplusFourApportionmentBuilder = lapcapplusFourApportionmentBuilder;
        }
        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);

            calcResult.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.Construct(resultsRequestDto, calcResult);

            return calcResult;
        }
    }
}