using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;

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
    TimeProvider timeProvider)
    : ICalculatorRunContextBuilder
{
    public async Task<CalculatorRunContext> Build(int runId, string? user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (string.IsNullOrWhiteSpace(user))
            throw new RunContextException(RunType.Calculator, runId, "User cannot be empty");

        var run = await GetCalculatorRunAsync(runId, cancellationToken);

        return new CalculatorRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = now,
            RelativeYear = run.RelativeYear,
            User = user
        };
    }

    private async Task<API.Data.DataModels.CalculatorRun> GetCalculatorRunAsync(int runId, CancellationToken ct)
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
}
