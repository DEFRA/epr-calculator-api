﻿using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public interface ICalcResultBuilder
    {
        public CalcResult Build(CalcResultsRequestDto resultsRequestDto);
    }
}
