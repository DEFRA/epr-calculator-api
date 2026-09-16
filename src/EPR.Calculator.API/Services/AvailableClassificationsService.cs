using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IAvailableClassificationsService
{
    Task<List<CalculatorRunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default);
}

public class AvailableClassificationsService(
    ApplicationDBContext context
) : IAvailableClassificationsService
{
    public async Task<List<CalculatorRunClassification>> GetAvailableClassificationsForRelativeYearAsync(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default)
    {
        var validStatuses = await DetermineAvailableClassificationsAsync(request, cancellationToken);

        if (validStatuses.Count == 0)
            return [];

        var allClassifications = await context.CalculatorRunClassifications
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return validStatuses
            .Join(
                allClassifications,
                status => status,
                classification => classification.Id,
                (_, classification) => classification)
            .ToList();
    }

    private async Task<bool> IsCurrentRunOlderThanOtherCompletedRuns(
        CalculatorRun currentRun,
        List<CalculatorRun> filteredRuns,
        CancellationToken cancellationToken)
    {
        var completedRuns = filteredRuns
            .Where(run => run.Classification.IsCompleted)
            .Select(run => run.Id)
            .ToList();

        var runs = await
            (from run in context.CalculatorRuns
                join calculatorRunBillingFileMetadata in context.CalculatorRunBillingFileMetadata
                    on run.Id equals calculatorRunBillingFileMetadata.CalculatorRunId
                where completedRuns.Contains(run.Id)
                select new
                {
                    RunId = run.Id,
                    calculatorRunBillingFileMetadata.BillingFileAuthorisedDate
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return runs.Where(run => run.BillingFileAuthorisedDate.HasValue)
            .Any(run => run.BillingFileAuthorisedDate!.Value >= currentRun.CreatedAt);
    }

    private async Task<List<RunClassification>> DetermineAvailableClassificationsAsync(
        CalcRelativeYearRequestDto request,
        CancellationToken cancellationToken)
    {
        var allRuns = await GetCalculatorRuns(request, cancellationToken);
        var run = allRuns.Single(r => r.Id == request.RunId);
        List<CalculatorRun> filteredRuns = [.. allRuns.Where(r => r.Id != request.RunId)];

        if (!filteredRuns.Exists(r => r.Classification.IsDesignated))
        {
            return
            [
                RunClassification.Initial,
                RunClassification.Test
            ];
        }

        if (filteredRuns.Exists(r => r.Classification is { IsDesignated: true, IsCompleted: false }))
        {
            return
            [
                RunClassification.Test
            ];
        }

        if (await IsCurrentRunOlderThanOtherCompletedRuns(run, filteredRuns, cancellationToken))
        {
            return
            [
                RunClassification.Test
            ];
        }

        return
        [
            RunClassification.Recalculation,
            RunClassification.Test
        ];
    }

    private async Task<List<CalculatorRun>> GetCalculatorRuns(CalcRelativeYearRequestDto request, CancellationToken cancellationToken)
    {
        var currentRuns = await context.CalculatorRuns
            .Where(run => run.RelativeYear == request.RelativeYearValue
                          && run.Classification != RunClassification.Deleted
                          && run.Classification != RunClassification.Errored
                          && run.Classification != RunClassification.Running)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return currentRuns;
    }
}
