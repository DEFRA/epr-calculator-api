using EPR.Calculator.API.Dtos;
using Microsoft.ApplicationInsights;

namespace EPR.Calculator.API.Services
{
    public interface ITransposePomAndOrgDataService
    {
        Task<bool> Transpose(CalcResultsRequestDto resultsRequestDto, TelemetryClient telemetryClient);
    }
}
