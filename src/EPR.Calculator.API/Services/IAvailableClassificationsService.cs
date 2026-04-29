using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Services;

public interface IAvailableClassificationsService
{
    Task<List<RunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default);
}
