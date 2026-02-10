using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface IAvailableClassificationsService
    {
        Task<List<CalculatorRunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default);
    }
}
