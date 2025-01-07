using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface ITransposePomAndOrgDataService
    {
        Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto);
    }
}
