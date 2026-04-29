using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class CalculationRunService(ApplicationDBContext context)
    : ICalculationRunService
{
    private static readonly ImmutableHashSet<RunClassification> DesignatedClassifications =
    [
        RunClassification.InitialRun,
        RunClassification.InitialRunCompleted,
        RunClassification.InterimRecalculationRun,
        RunClassification.InterimRecalculationRunCompleted,
        RunClassification.FinalRecalculationRun,
        RunClassification.FinalRecalculationRunCompleted,
        RunClassification.FinalRun,
        RunClassification.FinalRunCompleted
    ];

    public async Task<List<CalculatorRunDto>> GetDesignatedRunsByFinanialYear(RelativeYear relativeYear, CancellationToken cancellationToken = default)
    {
        return await context.CalculatorRuns
            .Where(run => run.RelativeYearValue == relativeYear.Value && DesignatedClassifications.Contains(run.Classification))
            .Select(CalcRunMapper.ToDto)
            .ToListAsync(cancellationToken);
    }
}
