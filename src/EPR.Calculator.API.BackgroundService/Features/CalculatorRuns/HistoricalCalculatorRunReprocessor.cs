using EPR.Calculator.API.Data;
using EPR.Calculator.API.BackgroundService.Builder;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;

/// <summary>
///     Re-runs <see cref="IResultBuilder" /> against an already-completed <see cref="Data.DataModels.CalculatorRun" />
///     to regenerate and store its result data, using the run's original input snapshot (org/pom/producer data,
///     and the LAPCAP/default-parameter masters it was linked to at submission time).
/// </summary>
/// <remarks>
///     Unlike <see cref="ICalculatorRunProcessor" />, this deliberately skips run-status mutation, data
///     initialization (which would overwrite the historical input snapshot with current data), and finalization -
///     it only re-runs the build/store step. It performs no check for existing result rows; callers must ensure
///     the target run has none, or duplicate rows / constraint violations will result.
/// </remarks>
public interface IHistoricalCalculatorRunReprocessor
{
    Task<CalcResult> ReprocessAsync(int runId, CancellationToken cancellationToken);
}

public class HistoricalCalculatorRunReprocessor(
    ApplicationDBContext dbContext,
    IParameterService parameterService,
    IResultBuilder resultBuilder,
    TimeProvider timeProvider)
    : IHistoricalCalculatorRunReprocessor
{
    public async Task<CalcResult> ReprocessAsync(int runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.CalculatorRuns
            .AsNoTracking()
            .SingleAsync(r => r.Id == runId, cancellationToken);

        var runContext = new CalculatorRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = timeProvider.GetUtcNow(),
            RelativeYear = run.RelativeYear,
            User = run.CreatedBy,
            DefaultParameters = await parameterService.GetDefaultParameters(runId)
        };

        return await resultBuilder.BuildAsync(runContext, cancellationToken);
    }
}
