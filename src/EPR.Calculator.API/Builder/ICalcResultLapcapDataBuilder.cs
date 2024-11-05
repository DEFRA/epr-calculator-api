﻿using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public interface ICalcResultLapcapDataBuilder
    {
        public CalcResultLapcapData Construct(CalcResultsRequestDto resultsRequestDto);
    }
}
