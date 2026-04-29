using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services;

public interface ICalculationRunService
{
    Task<List<CalculatorRunDto>> GetDesignatedRunsByFinanialYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
}
