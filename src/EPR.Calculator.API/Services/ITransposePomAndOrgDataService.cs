using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface ITransposePomAndOrgDataService
    {
        public void TransposeAsync(CalcResultsRequestDto resultsRequestDto);
    }
}
