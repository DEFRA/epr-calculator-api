using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;

namespace EPR.Calculator.API.Services;

public interface IAvailableClassificationsService
{
    Task<ImmutableHashSet<RunClassification>> GetAvailableClassifications(
        CalcRelativeYearRequestDto request,
        CancellationToken cancellationToken = default);
}

public class AvailableClassificationsService(
    ApplicationDBContext context
) : IAvailableClassificationsService
{
    public async Task<ImmutableHashSet<RunClassification>> GetAvailableClassifications(
        CalcRelativeYearRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var (run, officialRuns) = await GetRuns(request, cancellationToken);

        if (officialRuns.Count == 0)
        {
            return
            [
                RunClassification.Initial,
                RunClassification.Test
            ];
        }

        var hasIncompleteOfficialRun = officialRuns.Exists(r => r.RunClassification is { IsCompleted: false });
        var hasNewerCompletedRun = officialRuns.Exists(r => r.BillingFile?.SentAt >= run.CreatedAt);

        if (!hasIncompleteOfficialRun && !hasNewerCompletedRun)
        {
            return
            [
                RunClassification.Recalculation,
                RunClassification.Test
            ];
        }

        return
        [
            RunClassification.Test
        ];
    }

    private async Task<(CalculatorRunDto, ImmutableList<CalculatorRunDto>)> GetRuns(CalcRelativeYearRequestDto request, CancellationToken cancellationToken)
    {
        var runsForYear = await context.CalculatorRuns
            .Where(run => run.RelativeYear == request.RelativeYearValue
                          && (run.Id == request.RunId || RunClassificationHelper.OfficialClassifications.Contains(run.Classification)))
            .Select(CalcRunMapper.ToDto)
            .ToImmutableListAsync(cancellationToken);

        var requestedRun = runsForYear.Single(r => r.RunId == request.RunId);
        var otherRuns = runsForYear.Where(r => r.RunId != requestedRun.RunId).ToImmutableList();
        return (requestedRun, otherRuns);
    }
}
