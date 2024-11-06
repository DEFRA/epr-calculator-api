using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface ITransposePomAndOrgDataService
    {
        public void Transpose(CalcResultsRequestDto resultsRequestDto);
    }
}
