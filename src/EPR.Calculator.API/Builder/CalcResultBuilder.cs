﻿using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.CommsCost;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultBuilder : ICalcResultBuilder
    {
        private readonly ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder;
        private readonly ICalcResultDetailBuilder calcResultDetailBuilder;
        private readonly ICalcResultLapcapDataBuilder lapcapBuilder;
        private readonly ICalcResultOnePlusFourApportionmentBuilder lapcapplusFourApportionmentBuilder;
        private readonly ICalcResultCommsCostBuilder commsCostReportBuilder;
        private readonly ICalcResultLateReportingBuilder lateReportingBuilder;

        public CalcResultBuilder(
            ICalcResultDetailBuilder calcResultDetailBuilder,
            ICalcResultLapcapDataBuilder lapcapBuilder,
            ICalcResultParameterOtherCostBuilder calcResultParameterOtherCostBuilder,
            ICalcResultOnePlusFourApportionmentBuilder calcResultOnePlusFourApportionmentBuilder,
            ICalcResultCommsCostBuilder commsCostReportBuilder,
            ICalcResultLateReportingBuilder lateReportingBuilder)
        {
            this.calcResultDetailBuilder = calcResultDetailBuilder;
            this.lapcapBuilder = lapcapBuilder;
            this.commsCostReportBuilder = commsCostReportBuilder;
            this.lateReportingBuilder = lateReportingBuilder;
            this.calcResultParameterOtherCostBuilder = calcResultParameterOtherCostBuilder;
            this.lapcapplusFourApportionmentBuilder = calcResultOnePlusFourApportionmentBuilder;
        }

        public CalcResult Build(CalcResultsRequestDto resultsRequestDto)
        {
            var calcResult = new CalcResult();
            calcResult.CalcResultDetail = this.calcResultDetailBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLapcapData = this.lapcapBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultLateReportingTonnageData = this.lateReportingBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultParameterOtherCost = this.calcResultParameterOtherCostBuilder.Construct(resultsRequestDto);
            calcResult.CalcResultOnePlusFourApportionment = this.lapcapplusFourApportionmentBuilder.Construct(resultsRequestDto, calcResult);
            // calcResult.CalcResultCommsCostReportDetail = this.commsCostReportBuilder.Construct(resultsRequestDto.RunId);
            

            return calcResult;
        }
    }
}