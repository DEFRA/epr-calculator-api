using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.BillingRuns.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;

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
    IParameterService parameterService,
    TimeProvider timeProvider,
    ILogger<BillingRunContextBuilder> logger)
    : IBillingRunContextBuilder
{
    public async Task<BillingRunContext> Build(int runId, string? user, CancellationToken cancellationToken)
    {
        var preContext = await GetPreContext(runId, user, cancellationToken);
        return await BuildValidatedContext(preContext, cancellationToken);
    }

    private async Task<PreValidationContext> GetPreContext(int runId, string? user, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();

        // Must include the suggested billing instructions
        var run = await dbContext
            .CalculatorRuns
            .AsNoTracking()
            .Include(r => r.ProducerResultFileSuggestedBillingInstruction)
            .SingleOrDefaultAsync(r => r.Id == runId, ct);

        // Nothing to mark as Errored here
        if (run == null)
            throw new RunContextException(RunType.Billing, runId, "Run not found");

        return new PreValidationContext
        {
            StartedAt = now,
            User = user,
            Run = run
        };
    }

    private async Task<BillingRunContext> BuildValidatedContext(PreValidationContext preValidationContext, CancellationToken ct)
    {
        try
        {
            var validationResult = await new BillingRunContextValidator().ValidateAsync(preValidationContext, ct);

            if (!validationResult.IsValid)
                throw new RunContextException(RunType.Billing, preValidationContext.Run.Id, validationResult.ToString(", "));

            return new BillingRunContext
            {
                RunId = preValidationContext.Run.Id,
                RunName = preValidationContext.Run.Name.Trim(),
                ProcessingStartedAt = preValidationContext.StartedAt,
                RelativeYear = preValidationContext.Run.RelativeYear,
                User = preValidationContext.User!,
                AcceptedProducerIds = preValidationContext.AcceptedProducerIds,
                DefaultParameters = await parameterService.GetDefaultParameters(preValidationContext.Run.Id)
            };
        }
        catch
        {
            await MarkBillingRunAsFailed(preValidationContext.Run.Id, ct);
            throw;
        }
    }

    private async Task MarkBillingRunAsFailed(int runId, CancellationToken ct)
    {
        try
        {
            var run = await dbContext.CalculatorRuns.SingleAsync(r => r.Id == runId, ct);
            run.BillingRunStatus = BillingRunStatus.Errored;
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to mark billing run as {nameof(BillingRunStatus.Errored)}");
        }
    }

    public record PreValidationContext
    {
        public required DateTimeOffset StartedAt { get ; init ; }
        public required string? User { get ; init ; }
        public required CalculatorRun Run { get; init; }

        public ImmutableHashSet<int> AcceptedProducerIds => Run.ProducerResultFileSuggestedBillingInstruction
            .Where(p =>
                p.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && p.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel)
            .Select(p => p.ProducerId)
            .Distinct()
            .ToImmutableHashSet();
    }
}
