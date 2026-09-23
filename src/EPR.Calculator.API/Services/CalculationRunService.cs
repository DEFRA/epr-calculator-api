using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface ICalculationRunService
{
    Task<List<CalculatorRunDto>> GetDesignatedRunsByFinancialYear(RelativeYear relativeYear, CancellationToken cancellationToken = default);
}

public class CalculationRunService(ApplicationDBContext context)
    : ICalculationRunService
{
    public async Task<List<CalculatorRunDto>> GetDesignatedRunsByFinancialYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        return await context.CalculatorRuns
            .Where(run => run.RelativeYear == relativeYear && RunClassificationHelper.OfficialClassifications.Contains(run.Classification))
            .Select(CalcRunMapper.ToDto)
            .ToListAsync(cancellationToken);
    }
}
