﻿using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        public CalcResultParameterOtherCost Construct(CalcResultsRequestDto resultsRequestDto);
    }
}
