﻿using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Detail
{
    public interface ICalcResultDetailBuilder
    {
        public CalcResultDetail Construct(CalcResultsRequestDto resultsRequestDto);
    }
}