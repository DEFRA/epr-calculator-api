using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Services
{
    public interface ICalculationRunService
    {
        Task<List<ClassifiedCalculatorRunDto>> GetDesignatedRunsByFinanialYear(
            string financialYear,
            CancellationToken cancellationToken = default);
    }
}
