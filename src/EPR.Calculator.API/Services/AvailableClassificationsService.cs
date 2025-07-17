using EnumsNET;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public class AvailableClassificationsService(ApplicationDBContext context, ILogger<AvailableClassificationsService> logger) : IAvailableClassificationsService
{

    private readonly List<string> initialRunStatuses = new List<string>
        {
            RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)!,
            RunClassification.INITIAL_RUN_COMPLETED.AsString(EnumFormat.Description)!,
        };

    private readonly List<string> finalRecalculationRunStatuses = new List<string>
        {
            RunClassification.FINAL_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
            RunClassification.FINAL_RECALCULATION_RUN_COMPLETED.AsString(EnumFormat.Description)!,
        };

    private readonly List<string> finalRunStatuses = new List<string>
        {
            RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
            RunClassification.FINAL_RUN_COMPLETED.AsString(EnumFormat.Description)!,
        };

    public async Task<List<CalculatorRunClassification>> GetAvailableClassificationsForFinancialYearAsync(string financialYear, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> validStatuses = await this.DetermineAvailableClassificationsAsync(financialYear, cancellationToken);
            if (validStatuses.Count == 0)
            {
                return new();
            }

            return await context.CalculatorRunClassifications
                .Where(c => validStatuses.Contains(c.Status))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred whilst attempting to determine available classifications.");
            throw;
        }
    }

    private async Task<List<string>> DetermineAvailableClassificationsAsync(string financialYear, CancellationToken cancellationToken)
    {
        List<CalculatorRunClassification> currentClassifications = await this.GetCurrentClassifications(financialYear, cancellationToken);

        if (this.IsPreInitialRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INITIAL_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
            };
        }

        if (this.HasNeitherFinalRunNorFinalRecalculationRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
            };
        }

        if (this.HasFinalRecalculationButNoFinalRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.FINAL_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
            };
        }

        if (this.HasFinalRun(currentClassifications))
        {
            return new()
            {
                RunClassification.INTERIM_RECALCULATION_RUN.AsString(EnumFormat.Description)!,
                RunClassification.TEST_RUN.AsString(EnumFormat.Description)!,
            };
        }

        return new();
    }

    private async Task<List<CalculatorRunClassification>> GetCurrentClassifications(string financialYear, CancellationToken cancellationToken)
    {
        return await context.CalculatorRuns
            .Where(run => run.FinancialYearId == financialYear)
            .Join(
                context.CalculatorRunClassifications,
                run => run.CalculatorRunClassificationId,
                classification => classification.Id,
                (run, classification) => classification)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private bool IsPreInitialRun(List<CalculatorRunClassification> currentClassifications)
    {
        return currentClassifications.All(c => !this.initialRunStatuses.Contains(c.Status));
    }

    private bool HasNeitherFinalRunNorFinalRecalculationRun(List<CalculatorRunClassification> currentClassifications)
    {
        List<string> finalAndFinalRecalculationRunStatuses = this.finalRunStatuses.Concat(this.finalRecalculationRunStatuses).ToList();

        return currentClassifications.All(c => !finalAndFinalRecalculationRunStatuses.Contains(c.Status));
    }

    private bool HasFinalRecalculationButNoFinalRun(List<CalculatorRunClassification> currentClassifications)
    {
        return currentClassifications.Any(c => this.finalRecalculationRunStatuses.Contains(c.Status))
            && currentClassifications.All(c => !this.finalRunStatuses.Contains(c.Status));
    }

    private bool HasFinalRun(List<CalculatorRunClassification> currentClassifications)
    {
        return currentClassifications.Any(c => this.finalRunStatuses.Contains(c.Status));
    }
}
