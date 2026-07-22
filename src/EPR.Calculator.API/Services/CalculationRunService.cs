using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
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
    private static readonly ImmutableHashSet<int> DesignatedClassificationIds =
    [
        (int)RunClassification.INITIAL_RUN,
        (int)RunClassification.INITIAL_RUN_COMPLETED,
        (int)RunClassification.INTERIM_RECALCULATION_RUN,
        (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED,
        (int)RunClassification.FINAL_RECALCULATION_RUN,
        (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
        (int)RunClassification.FINAL_RUN,
        (int)RunClassification.FINAL_RUN_COMPLETED
    ];

    public async Task<List<CalculatorRunDto>> GetDesignatedRunsByFinancialYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        return await context.CalculatorRuns
            .Where(run => run.RelativeYear == relativeYear
                          && DesignatedClassificationIds.Contains(run.CalculatorRunClassificationId))
            .Select(CalcRunMapper.ToDto)
            .ToListAsync(cancellationToken);
    }
}
