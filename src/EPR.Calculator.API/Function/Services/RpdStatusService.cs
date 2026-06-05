using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IRpdStatusService
    {
        Task<RunClassification> UpdateRpdStatus(int runId, string createdBy, CancellationToken timeout);
    }

    [ExcludeFromCodeCoverage(Justification = "Soon to be removed")]
    public class RpdStatusService(
        ApplicationDBContext context,
        IRpdStatusDataValidator validator,
        ICalculatorRunOrgData calculatorRunOrgData,
        ICalculatorRunPomData calculatorRunPomData,
        ILogger<RpdStatusService> logger)
        : IRpdStatusService
    {
        /// <inheritdoc/>
        public async Task<RunClassification> UpdateRpdStatus(
            int runId,
            string createdBy,
            CancellationToken timeout)
        {
            var calcRun = await context.CalculatorRuns.SingleOrDefaultAsync(
                run => run.Id == runId,
                timeout);
            var runClassifications = await context.CalculatorRunClassifications
                .ToListAsync(timeout);

            var validationResult = validator.IsValidRun(calcRun, runId, runClassifications);

            if (!validationResult.isValid)
                throw new ValidationException(validationResult.ToString());

            var vr = validator.IsValidSuccessfulRun(runId);

            if (!vr.isValid)
                throw new ValidationException(vr.ToString());

            var relativeYear = calcRun!.RelativeYear;

             await LoadOrgAndPomData(runId, createdBy, relativeYear, calcRun, runClassifications, timeout);
             return RunClassification.RUNNING;
        }

        private Task LoadOrgAndPomData(int runId, string createdBy, RelativeYear relativeYear, CalculatorRun calcRun, List<CalculatorRunClassification> runClassifications, CancellationToken cancellationToken) =>
            logger.LogDuration(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await calculatorRunOrgData.LoadOrgDataForCalcRun(runId, relativeYear, createdBy, cancellationToken);
                    await calculatorRunPomData.LoadPomDataForCalcRun(runId, relativeYear, createdBy, cancellationToken);

                    calcRun.CalculatorRunClassificationId = runClassifications.Single(x => x.Status == nameof(RunClassification.RUNNING)).Id;

                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }
            });
    }
}
