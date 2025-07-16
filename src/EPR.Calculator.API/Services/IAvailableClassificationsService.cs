using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Services
{
    public interface IAvailableClassificationsService
    {
        Task<List<CalculatorRunClassification>> GetAvailableClassificationsForFinancialYearAsync(string financialYear, CancellationToken cancellationToken = default);
    }
}
