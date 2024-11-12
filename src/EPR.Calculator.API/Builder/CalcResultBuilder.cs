using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.CommsCost;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultCommsCostBuilder commsCostReportBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;
        private readonly ICalcRunLaDisposalCostBuilder laDisposalCostBuilder;

        public CalcResultBuilder(
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcRunLaDisposalCostBuilder calcRunLaDisposalCostBuilder)
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            this.laDisposalCostBuilder = calcRunLaDisposalCostBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto); ;
            calcResult.CalcResultLateReportingTonnageData = this.lateReportingBuilder.Construct(resultsRequestDto);
            // calcResult.CalcResultCommsCostReportDetail = commsCostReportBuilder.Construct(resultsRequestDto.RunId);
            calcResult.CalcResultParameterOtherCost = this.calcResultParameterOtherCostBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLaDisposalCostData = this.laDisposalCostBuilder.Construct(resultsRequestDto, calcResult);

            return calcResult;
        }
    }
}
