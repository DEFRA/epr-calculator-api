using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcRunLaDisposalCostBuilder laDisposalCostBuilder;
        public CalcResultBuilder(ICalcResultDetailBuilder calcResultDetailBuilder, ICalcResultLapcapDataBuilder lapcapBuilder, ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder) 
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLaDisposalCostData = this.laDisposalCostBuilder.Construct(resultsRequestDto, calcResult);

            return calcResult;
        }
    }
}
