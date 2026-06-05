using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRun.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.BillingRun.Contexts;

/// <summary>
///     Creates <see cref="BillingRunContext">BillingRunContexts</see> for use with the <see cref="BillingRunProcessor" />.
/// </summary>
public interface IBillingRunContextBuilder
{
    /// <summary>
    ///     Creates a valid <see cref="BillingRunContext" /> for the specified run/user.
    /// </summary>
    /// <exception cref="RunContextException">
    ///     If the underlying <see cref="CalculatorRun" /> is not in a valid state for billing run processing.
    /// </exception>
    Task<BillingRunContext> Build(int runId, string? user, CancellationToken cancellationToken);
}

public class BillingRunContextBuilder(
    ApplicationDBContext dbContext,
    TimeProvider timeProvider)
    : IBillingRunContextBuilder
{
    public async Task<BillingRunContext> Build(int runId, string? user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        if (string.IsNullOrWhiteSpace(user))
            throw new RunContextException(RunType.Billing, runId, "User cannot be empty");

        var calculatorRun = await GetCalculatorRunAsync(runId, cancellationToken);
        var acceptedProducerIds = await GetAcceptedProducerIdsAsync(runId, cancellationToken);

        return new BillingRunContext
        {
            RunId = calculatorRun.Id,
            RunName = calculatorRun.Name.Trim(),
            ProcessingStartedAt = now,
            RelativeYear = calculatorRun.RelativeYear,
            User = user,
            AcceptedProducerIds = acceptedProducerIds
        };
    }

    private async Task<API.Data.DataModels.CalculatorRun> GetCalculatorRunAsync(int runId, CancellationToken ct)
    {
        var run = await dbContext
            .CalculatorRuns
            .AsNoTracking()
            .Include(r => r.ProducerResultFileSuggestedBillingInstruction)
            .SingleOrDefaultAsync(r => r.Id == runId, ct);

        if (run == null) throw new RunContextException(RunType.Billing, runId, "Run not found");

        var validationResult = await new BillingRunValidator().ValidateAsync(run, ct);

        if (!validationResult.IsValid) throw new RunContextException(RunType.Billing, runId, validationResult.ToString(", "));

        return run;
    }

    private async Task<ImmutableHashSet<int>> GetAcceptedProducerIdsAsync(int runId, CancellationToken ct)
    {
        var acceptedProducers = await dbContext
            .ProducerResultFileSuggestedBillingInstruction
            .Where(p =>
                p.CalculatorRunId == runId
                && p.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && p.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel)
            .Select(p => p.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync(ct);

        if (acceptedProducers.Count == 0) throw new RunContextException(RunType.Billing, runId, "No producers have been accepted for this billing run");

        return acceptedProducers;
    }
}
