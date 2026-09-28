using EPR.Calculator.API.BackgroundService.Exceptions;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;

/// <summary>
///     Creates <see cref="CalculatorRunContext">CalculatorRunContexts</see> for use with the
///     <see cref="CalculatorRunProcessor" />.
/// </summary>
public interface ICalculatorRunContextBuilder
{
    /// <summary>
    ///     Creates a valid <see cref="CalculatorRunContext" /> for the specified run/user.
    /// </summary>
    /// <exception cref="RunContextException">
    ///     If the underlying <see cref="CalculatorRun" /> is not in a valid state for calculator run processing.
    /// </exception>
    Task<CalculatorRunContext> Build(int runId, string? user, CancellationToken cancellationToken);
}

public class CalculatorRunContextBuilder(
    ApplicationDBContext dbContext,
    IParameterService parameterService,
    TimeProvider timeProvider,
    ILogger<CalculatorRunContextBuilder> logger)
    : ICalculatorRunContextBuilder
{
    [ActivityTrace]
    public async Task<CalculatorRunContext> Build(int runId, string? user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (string.IsNullOrWhiteSpace(user))
            throw new RunContextException(RunType.Calculator, runId, "User cannot be empty");

        try
        {
            var run = await GetValidRunAsync(runId, cancellationToken);

            return new CalculatorRunContext
            {
                RunId = run.Id,
                RunName = run.Name.Trim(),
                ProcessingStartedAt = now,
                RelativeYear = run.RelativeYear,
                User = user,
                DefaultParameters = await parameterService.GetDefaultParameters(runId)
            };
        }
        catch
        {
            await MarkRunAsErrored(runId, cancellationToken);
            throw;
        }
    }

    private async Task<CalculatorRun> GetValidRunAsync(int runId, CancellationToken ct)
    {
        var run = await dbContext
            .CalculatorRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == runId, ct);

        if (run == null)
            throw new RunContextException(RunType.Calculator, runId, "Run not found");

        var validationResult = await new CalculatorRunValidator().ValidateAsync(run, ct);

        if (!validationResult.IsValid)
            throw new RunContextException(RunType.Calculator, runId, validationResult.ToString("|"));

        return run;
    }

    private async Task MarkRunAsErrored(int runId, CancellationToken ct)
    {
        try
        {
            await dbContext.CalculatorRuns
                .Where(r => r.Id == runId)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.CalculationRunStatus, CalculationRunStatus.Errored), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to mark run {{RunId}} calculation as {nameof(CalculationRunStatus.Errored)}", runId);
        }
    }
}
