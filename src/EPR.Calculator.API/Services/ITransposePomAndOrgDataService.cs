﻿using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface ITransposePomAndOrgDataService
    {
        public Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto);
    }
}
